import {
  PublicClientApplication,
  AccountInfo,
  AuthenticationResult,
  LogLevel,
} from "@azure/msal-browser";
import { EApiFunctionTypes, REQUEST_TYPE } from "../types";
import axios, { AxiosResponse } from "axios";
import { parseChallenges } from "../utils";
import { getFrontendOptions, loadCustomCss } from "./frontend-options-service";
import { Mutex } from "async-mutex";

export type TMsalInfo = {
  msalInstance: PublicClientApplication;
  backendClientId: string;
};

const getMsalInfoMutex = new Mutex();
let msalInfoCache: TMsalInfo | undefined = undefined;
export const getMsalInfo = async (): Promise<TMsalInfo> => {
  return await getMsalInfoMutex.runExclusive(async () => {
    if (msalInfoCache) {
      return msalInfoCache;
    }

    const frontendOptions = await getFrontendOptions();

    // Load custom CSS if configured
    loadCustomCss(frontendOptions.customCssUrl);

    msalInfoCache = {
      msalInstance: new PublicClientApplication({
        auth: {
          clientId: frontendOptions.frontendClientId,
          authority: `https://login.microsoftonline.com/${frontendOptions.tenantId}`,
        },
        system: {
          loggerOptions: {
            logLevel: LogLevel.Warning,
            loggerCallback: (level, message, containsPii) => {
              if (containsPii) {
                return;
              }
              switch (level) {
                case LogLevel.Error:
                  console.error(message);
                  return;
                case LogLevel.Info:
                  console.info(message);
                  return;
                case LogLevel.Verbose:
                  console.debug(message);
                  return;
                case LogLevel.Warning:
                  console.warn(message);
                  return;
              }
            },
            piiLoggingEnabled: false,
          },
        },
      }),
      backendClientId: frontendOptions.backendClientId,
    };
    await msalInfoCache.msalInstance.initialize();
    return msalInfoCache;
  });
};

// Resolves the account MSAL should use for every token acquisition. Prefers the
// explicitly-set active account. On multiple signed in accounts, prompts the
// user to select one.
export const getActiveMsalAccount = async (): Promise<AccountInfo> => {
  const msalInfo = await getMsalInfo();
  const activeAccount = msalInfo.msalInstance.getActiveAccount();
  if (activeAccount) {
    return activeAccount;
  }

  const accounts = msalInfo.msalInstance.getAllAccounts();
  if (accounts.length === 0) {
    throw new Error("User not signed in");
  }
  if (accounts.length === 1) {
    msalInfo.msalInstance.setActiveAccount(accounts[0]);
    return accounts[0];
  }

  await msalInfo.msalInstance.loginRedirect({
    prompt: "select_account",
    scopes: [`api://${msalInfo.backendClientId}/Access`],
  });
  // Should never throw since loginRedirect navigates away from the page.
  throw new Error("Multiple accounts cached - prompting account selection");
};

export const sendAxiosRequest = async <T, D = unknown>(
  url: string,
  requestType: REQUEST_TYPE,
  bearerToken: string,
  body?: D
): Promise<AxiosResponse<T, D>> => {
  const header = {
    Authorization: `Bearer ${bearerToken}`,
  };

  switch (requestType) {
    case REQUEST_TYPE.GET:
      return await axios.get<T>(url, { headers: header });
    case REQUEST_TYPE.POST:
      return await axios.post<T>(url, body, { headers: header });
    case REQUEST_TYPE.PUT:
      return await axios.put<T>(url, body, { headers: header });
    case REQUEST_TYPE.DELETE:
      return await axios.delete<T>(url, { headers: header });
    default:
      throw new Error("Invalid request type");
  }
};

export const authenticateRequest = async <T, D = undefined>(
  url: string,
  requestType: REQUEST_TYPE,
  redirectState?: string,
  body?: D
): Promise<T> => {
  let response: AxiosResponse<T, D> | undefined;
  const token = await getBearerToken();
  try {
    response = await sendAxiosRequest<T, D>(url, requestType, token, body);
  } catch (error) {
    if (!axios.isAxiosError(error)) {
      throw error;
    }
    response = error.response;
    if (!response) {
      console.log("No Axios error response returned", response);
      throw error;
    }
    if (response.status === 401) {
      if (!response.headers["www-authenticate"]) {
        throw new Error("Authentication failed - no challenge provided", {
          cause: error,
        });
      }
      const wwwAuthenticateHeader = parseChallenges(
        response.headers["www-authenticate"]
      );

      const msalInfo = await getMsalInfo();
      const activeAccount = await getActiveMsalAccount();
      await msalInfo.msalInstance.acquireTokenRedirect({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://${msalInfo.backendClientId}/Access`],
        state: redirectState,
        account: activeAccount,
        loginHint: activeAccount.username,
      });
    } else {
      throw error;
    }
  }
  return response?.data;
};

const getBearerToken = async (): Promise<string> => {
  const msalInfo = await getMsalInfo();
  const activeAccount = await getActiveMsalAccount();

  const request = {
    scopes: [`api://${msalInfo.backendClientId}/Access`],
    account: activeAccount,
  };

  const authResult = await msalInfo.msalInstance
    .acquireTokenSilent(request)
    .catch(async (error: Error) => {
      console.warn("acquire token silently failed", error);
      await msalInfo.msalInstance.acquireTokenRedirect({
        scopes: [`api://${msalInfo.backendClientId}/Access`],
        account: activeAccount,
        loginHint: activeAccount.username,
      });
    });
  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};

export const getGraphBearerToken = async (): Promise<string> => {
  const msalInfo = await getMsalInfo();
  const activeAccount = await getActiveMsalAccount();

  const request = {
    scopes: [`https://graph.microsoft.com/User.Read`],
    account: activeAccount,
  };

  const authResult = await msalInfo.msalInstance
    .acquireTokenSilent(request)
    .catch(async (error: Error) => {
      console.warn("acquire token silently failed", error);
      await msalInfo.msalInstance.acquireTokenRedirect({
        scopes: [`https://graph.microsoft.com/User.Read`],
        account: activeAccount,
        loginHint: activeAccount.username,
      });
    });
  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};

// Memoize MSAL redirect response to let every caller observe the same one
let redirectPromiseCache: Promise<AuthenticationResult | null> | undefined =
  undefined;

export const handleRedirectPromise =
  (): Promise<AuthenticationResult | null> => {
    redirectPromiseCache ??= (async () => {
      const msalInfo = await getMsalInfo();
      const authenticationResult =
        await msalInfo.msalInstance.handleRedirectPromise();
      if (authenticationResult?.account) {
        msalInfo.msalInstance.setActiveAccount(authenticationResult.account);
      }
      return authenticationResult;
    })();
    return redirectPromiseCache;
  };

export const getPendingAction = (
  authenticationResult: AuthenticationResult
): EApiFunctionTypes => {
  if (authenticationResult?.state) {
    switch (authenticationResult?.state) {
      case EApiFunctionTypes.DISMISS_USER_RISK:
        return EApiFunctionTypes.DISMISS_USER_RISK;
      case EApiFunctionTypes.CREATE_TAP:
        return EApiFunctionTypes.CREATE_TAP;
      case EApiFunctionTypes.REVOKE_TAP:
        return EApiFunctionTypes.REVOKE_TAP;
      case EApiFunctionTypes.PASSWORD_RESET:
        return EApiFunctionTypes.PASSWORD_RESET;
      default:
        throw new Error("invalide state provided");
    }
  } else {
    throw new Error("No state provided");
  }
};
