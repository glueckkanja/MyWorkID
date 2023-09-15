import { PublicClientApplication, Configuration } from "@azure/msal-browser";
import { REQUEST_TYPE, TFrontendOptions } from "../types";
import axios, { AxiosError, AxiosResponse } from "axios";
import { parseChallenges } from "../utils";

var _msalInstance: PublicClientApplication | undefined;
var _backendClientId: string | undefined;

export const getMsalInstance = async (): Promise<PublicClientApplication> => {
  var frontendOptions = await axios.get<TFrontendOptions>(
    "https://localhost:7093/FrontendOptions"
  );
  console.log("response", frontendOptions)
  console.log("responseData", frontendOptions.data)
  if (!_msalInstance) {
    _backendClientId = frontendOptions.data.backendClientId;
    var msalConfig: Configuration = {
      auth: {
        clientId: frontendOptions.data.frontendClientId,
        authority: `https://login.microsoftonline.com/${frontendOptions.data.tenantId}`,
      },
    };
    _msalInstance = new PublicClientApplication(msalConfig);
  }

  return _msalInstance;
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
  let msalInstance = await getMsalInstance();
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
      const tokenResponse = await msalInstance.acquireTokenRedirect({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://${_backendClientId}/Access`],
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
  let msalInstance = await getMsalInstance();
  const accounts = msalInstance.getAllAccounts();

  if (accounts.length === 0) {
    throw new Error("User not signed in");
  }

  const request = {
    scopes: [`api://${_backendClientId}/Access`],
    account: accounts[0],
  };

  const authResult = await msalInstance
    .acquireTokenSilent(request)
    .catch((error: Error) => {
      console.warn("acquire token silently failed", error);
      msalInstance.acquireTokenRedirect({
        scopes: [`api://${_backendClientId}/Access`],
      });
    });

  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};
