import {
  PublicClientApplication,
  AuthenticationResult,
  LogLevel,
} from "@azure/msal-browser";
import { EApiFunctionTypes, REQUEST_TYPE, TFunctionResult } from "../types";
import axios, { AxiosResponse } from "axios";
import { parseChallenges } from "../utils";
import { generateTAP, dismissUserRisk, checkResetPasswordClaim } from "./ApiService";
import { getFrontendOptions } from "./FrontendOptionsService";
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


    let frontendOptions = await getFrontendOptions();

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

export const sendAxiosRequest = async <T>(
  url: string,
  requestType: REQUEST_TYPE,
  bearerToken: string,
  body?: any
): Promise<AxiosResponse<T, any>> => {
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
    default:
      throw new Error("Invalid request type");
  }
};

export const authenticateRequest = async <T>(
  url: string,
  requestType: REQUEST_TYPE,
  redirectState?: string,
  body?: any
): Promise<T> => {
  let response: AxiosResponse<T, any> | undefined;
  const token = await getBearerToken();
  try {
    response = await sendAxiosRequest<T>(url, requestType, token, body);
  } catch (error: any) {
    response = error.response;
    if (!response) {
      console.log("No Axios error response returned", response);
      throw error;
    }
    if (response.status === 401) {
      if (!response.headers["www-authenticate"]) {
        throw new Error("Authentication failed - no challenge provided");
      }
      const wwwAuthenticateHeader = parseChallenges(
        response.headers["www-authenticate"]
      );

      var msalInfo = await getMsalInfo();
      await msalInfo.msalInstance.acquireTokenRedirect({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://${msalInfo.backendClientId}/Access`],
        state: redirectState,
      });
    } else {
      throw error;
    }
  }
  return response?.data;
};

const getBearerToken = async (): Promise<string> => {
  var msalInfo = await getMsalInfo();
  const accounts = msalInfo.msalInstance.getAllAccounts();

  if (accounts.length === 0) {
    throw new Error("User not signed in");
  }

  const request = {
    scopes: [`api://${msalInfo.backendClientId}/Access`],
    account: accounts[0],
  };

  const authResult = await msalInfo.msalInstance
    .acquireTokenSilent(request)
    .catch(async (error: Error) => {
      console.warn("acquire token silently failed", error);
      await msalInfo.msalInstance.acquireTokenRedirect({
        scopes: [`api://${msalInfo.backendClientId}/Access`],
      });
    });
  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};

export const getGraphBearerToken = async (): Promise<string> => {
  var msalInfo = await getMsalInfo();
  const accounts = msalInfo.msalInstance.getAllAccounts();

  if (accounts.length === 0) {
    throw new Error("User not signed in");
  }

  const request = {
    scopes: [`https://graph.microsoft.com/User.Read`],
    account: accounts[0],
  };

  const authResult = await msalInfo.msalInstance
    .acquireTokenSilent(request)
    .catch(async (error: Error) => {
      console.warn("acquire token silently failed", error);
      await msalInfo.msalInstance.acquireTokenRedirect({
        scopes: [`https://graph.microsoft.com/User.Read`],
      });
    });
  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};

export const handleRedirectPromise =
  async (): Promise<AuthenticationResult | null> => {
    var msalInfo = await getMsalInfo();
    return await msalInfo.msalInstance.handleRedirectPromise();
  };

export const getPendingAction = (
  authenticationResult: AuthenticationResult
): TFunctionResult<any> => {
  if (authenticationResult?.state) {
    switch (authenticationResult?.state) {
      case EApiFunctionTypes.DISMISS_USER_RISK:
        return {
          status: "pending",
          dataType: EApiFunctionTypes.DISMISS_USER_RISK,
        };
      case EApiFunctionTypes.CREATE_TAP:
        return {
          status: "pending",
          dataType: EApiFunctionTypes.CREATE_TAP,
        };
      default:
        throw new Error("invalide state provided");
    }
  } else {
    return {
      status: "error",
      errorMessage: "No state provided",
      dataType: EApiFunctionTypes.UNKNOWN,
    };
  }
};

export const handleActionAuthRedirect = async (
  authenticationResult: AuthenticationResult
): Promise<TFunctionResult<any>> => {
  if (authenticationResult?.state) {
    try {
      switch (authenticationResult?.state) {
        case EApiFunctionTypes.DISMISS_USER_RISK:
          return await dismissUserRisk();
        case EApiFunctionTypes.CREATE_TAP:
          return await generateTAP();
        case EApiFunctionTypes.PASSWORD_RESET:
          return await checkResetPasswordClaim();
        default:
          throw new Error("invalide state provided");
      }
    } catch (error: any) {
      switch (authenticationResult?.state) {
        case EApiFunctionTypes.DISMISS_USER_RISK:
          return {
            status: "error",
            errorMessage: error.message,
            dataType: EApiFunctionTypes.DISMISS_USER_RISK,
          };
        case EApiFunctionTypes.CREATE_TAP:
          return {
            status: "error",
            errorMessage: error.message,
            dataType: EApiFunctionTypes.CREATE_TAP,
            data: { temporaryAccessPassword: "ERROR" },
          };
          case EApiFunctionTypes.PASSWORD_RESET:
            return {
              status: "error",
              errorMessage: error.message,
              dataType: EApiFunctionTypes.PASSWORD_RESET,
            }
        default:
          throw new Error("invalide state provided");
      }
    }
  } else {
    return {
      status: "error",
      errorMessage: "No state provided",
      dataType: EApiFunctionTypes.UNKNOWN,
    };
  }
};
