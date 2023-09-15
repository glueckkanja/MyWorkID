import { PublicClientApplication, Configuration } from "@azure/msal-browser";
import { REQUEST_TYPE, TFrontendOptions } from "../types";
import axios, { AxiosError, AxiosResponse } from "axios";
import { parseChallenges } from "../utils";

var _msalInfo: TMsalInfo | undefined;

export type TMsalInfo = {
  msalInstance: PublicClientApplication;
  backendClientId: string;
};

export const getMsalInfo = async (): Promise<TMsalInfo> => {
  if (!_msalInfo) {
    var frontendOptions = await axios.get<TFrontendOptions>(
      "https://localhost:7093/FrontendOptions"
    );
    var msalConfig: Configuration = {
      auth: {
        clientId: frontendOptions.data.frontendClientId,
        authority: `https://login.microsoftonline.com/${frontendOptions.data.tenantId}`,
      },
    };

    _msalInfo = _msalInfo ?? {
      msalInstance: new PublicClientApplication(msalConfig),
      backendClientId: frontendOptions.data.backendClientId,
    }; // null check necessary so the debug process that follows the fucking main process doesnt fuck shit up completly :)
  }

  return _msalInfo;
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
  let msalInfo = await getMsalInfo();
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
      const tokenResponse = await msalInfo.msalInstance.acquireTokenRedirect({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://${msalInfo.backendClientId}/Access`],
        state: redirectState,
      });
      // if (tokenResponse.accessToken) {
      //   response = await sendAxiosRequest<T>(
      //     url,
      //     requestType,
      //     tokenResponse.accessToken,
      //     body
      //   );
      // }
    }
  }

  // Handle Auth challenge

  return response?.data;
};

const getBearerToken = async (): Promise<string> => {
  let msalInfo = await getMsalInfo();
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
    .catch((error: Error) => {
      console.warn("acquire token silently failed", error);
      msalInfo.msalInstance.acquireTokenRedirect({
        scopes: [`api://${msalInfo.backendClientId}/Access`],
      });
    });

  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};
