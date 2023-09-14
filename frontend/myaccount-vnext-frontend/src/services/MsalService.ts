import { PublicClientApplication, Configuration } from "@azure/msal-browser";
import { REQUEST_TYPE } from "../types";
import axios, { AxiosError, AxiosResponse } from "axios";
import { parseChallenges } from "../utils";

var _msalInstance: PublicClientApplication | undefined;

export const getMsalInstance = (): Promise<PublicClientApplication> => {
  return new Promise((resolve) => {
    if (!_msalInstance) {
      var msalConfig: Configuration = {
        auth: {
          clientId: "ebd71a9c-1e84-4d8d-a628-f7caed6c1102",
          authority: `https://login.microsoftonline.com/a9ae459a-6068-4a03-915a-7031507edbc1`,
        },
      };
      _msalInstance = new PublicClientApplication(msalConfig);
    }
    resolve(_msalInstance);
  });
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
  body?: any
): Promise<T | undefined> => {
  let msalInstance = await getMsalInstance();
  let response: AxiosResponse<T, any> | undefined;
  let token = await getBearerToken();
  try {
    response = await sendAxiosRequest<T>(url, requestType, token, body);
  } catch (error: any) {
    response = error.response;
    if(!response){
      throw new Error("No Axios error response returned")
    }
    if (response.status === 401) {
      if (!response.headers["www-authenticate"]) {
        throw new Error("Authentication failed - no chanllenge provided");
      }
      let wwwAuthenticateHeader = parseChallenges(
        response.headers["www-authenticate"]
      );
      const tokenResponse = await msalInstance.acquireTokenPopup({
        claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
        scopes: [`api://fc97e872-bf3b-4531-82b0-8b85272982e2/Access`],
      });
      if (tokenResponse.accessToken) {
        response = await sendAxiosRequest<T>(
          url,
          requestType,
          tokenResponse.accessToken,
          body
        );
      }
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
    scopes: [`api://fc97e872-bf3b-4531-82b0-8b85272982e2/Access`],
    account: accounts[0],
  };

  const authResult = await msalInstance
    .acquireTokenSilent(request)
    .catch((error: Error) => {
      console.warn("acquire token silently failed", error);
      msalInstance.acquireTokenRedirect({
        scopes: [`api://fc97e872-bf3b-4531-82b0-8b85272982e2/Access`],
      });
    });

  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};
