import { PublicClientApplication } from "@azure/msal-browser";
import { REQUEST_TYPE } from "../types";
import axios, { AxiosResponse } from "axios";
import { parseChallenges } from "../utils";
import { Strings } from "../Strings";
import { dismissUserRisk } from "./ApiService";

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

const sendAxiosRequest = async <T>(
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
): Promise<T | undefined> => {
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
        throw new Error("Authentication failed - no chanllenge provided");
      }
      let wwwAuthenticateHeader = parseChallenges(
        response.headers["www-authenticate"]
      );
      await MSAL_INFO.msalInstance.acquireTokenRedirect({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://${MSAL_INFO.backendClientId}/Access`],
        state: redirectState,
      });
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

export const handleActionAuthRedirect = (): void => {
  MSAL_INFO.msalInstance.handleRedirectPromise().then((res) => {
    if (res?.state) {
      switch (res.state) {
        case Strings.DISMISS_USER_RISK:
          dismissUserRisk();
          break;
        default:
          return;
      }
    }
  });
};
