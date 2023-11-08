import {
  PublicClientApplication,
  AuthenticationResult,
} from "@azure/msal-browser";
import { EApiFunctionTypes, REQUEST_TYPE, TFunctionResult } from "../types";
import axios, { AxiosResponse } from "axios";
import { parseChallenges } from "../utils";
import { generateTAP, dismissUserRisk } from "./ApiService";

export type TMsalInfo = {
  msalInstance: PublicClientApplication;
  backendClientId: string;
};

export const MSAL_INFO: TMsalInfo = {
  msalInstance: new PublicClientApplication({
    auth: {
      clientId: window.settings.frontendClientId,
      authority: `https://login.microsoftonline.com/${window.settings.tenantId}`,
    },
  }),
  backendClientId: window.settings.backendClientId,
};

export const sendAxiosRequest = async <T>(
  url: string,
  requestType: REQUEST_TYPE,
  bearerToken: string,
  body?: any
): Promise<AxiosResponse<T, any>> => {
  let header = {
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
  let token = await getBearerToken();
  try {
    response = await sendAxiosRequest<T>(url, requestType, token, body);
  } catch (error: any) {
    response = error.response;
    if (!response) {
      console.log("No Axios error response returned", response);
      throw new Error("No Axios error response returned");
    }
    if (response.status === 401) {
      if (!response.headers["www-authenticate"]) {
        throw new Error("Authentication failed - no challenge provided");
      }
      let wwwAuthenticateHeader = parseChallenges(
        response.headers["www-authenticate"]
      );
      await MSAL_INFO.msalInstance.acquireTokenRedirect({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://${MSAL_INFO.backendClientId}/Access`],
        state: redirectState,
      });
    } else {
      throw error;
    }
  }
  return response?.data;
};

const getBearerToken = async (): Promise<string> => {
  const accounts = MSAL_INFO.msalInstance.getAllAccounts();

  if (accounts.length === 0) {
    throw new Error("User not signed in");
  }

  const request = {
    scopes: [`api://${MSAL_INFO.backendClientId}/Access`],
    account: accounts[0],
  };

  const authResult = await MSAL_INFO.msalInstance
    .acquireTokenSilent(request)
    .catch(async (error: Error) => {
      console.warn("acquire token silently failed", error);
      await MSAL_INFO.msalInstance.acquireTokenRedirect({
        scopes: [`api://${MSAL_INFO.backendClientId}/Access`],
      });
    });
  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};

export const getGraphBearerToken = async (): Promise<string> => {
  const accounts = MSAL_INFO.msalInstance.getAllAccounts();

  if (accounts.length === 0) {
    throw new Error("User not signed in");
  }

  const request = {
    scopes: [`https://graph.microsoft.com/User.Read`],
    account: accounts[0],
  };

  const authResult = await MSAL_INFO.msalInstance
    .acquireTokenSilent(request)
    .catch(async (error: Error) => {
      console.warn("acquire token silently failed", error);
      await MSAL_INFO.msalInstance.acquireTokenRedirect({
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
    return await MSAL_INFO.msalInstance.handleRedirectPromise();
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
