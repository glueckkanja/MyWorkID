import { PublicClientApplication, Configuration } from "@azure/msal-browser";
import { REQUEST_TYPE } from "../types";
import axios, { AxiosResponse } from "axios";
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

export const authenticateRequest = async <T>(
  url: string,
  requestType: REQUEST_TYPE,
  body?: any
): Promise<T | undefined> => {
  let msalInstance = await getMsalInstance();

  let header = {
    Authorization: `Bearer ${await getBearerToken()}`,
  };

  let response: AxiosResponse<T, any> | undefined;

  switch (requestType) {
    case REQUEST_TYPE.GET:
      response = await axios.get<T>(url, { headers: header });
      break;
    case REQUEST_TYPE.POST:
      response = await axios.post<T>(url, body, { headers: header });
      break;
      case REQUEST_TYPE.PUT:
        response = await axios.put<T>(url, body, { headers: header });
        break;
    default:
      throw new Error("Invalid request type");
  }

  if (response.status === 401) {
    let wwwAuthenticateHeader = parseChallenges(
      response.headers["www-authenticate"]
    );

    const tokenResponse = await msalInstance.acquireTokenPopup({
      claims: window.atob(wwwAuthenticateHeader.claims), // decode the base64 string
      scopes: [
        `api://fc97e872-bf3b-4531-82b0-8b85272982e2/Access`,
      ],
      redirectUri: "/redirect",
    });

    if (tokenResponse.accessToken) {
      let header = {
        Authorization: `Bearer ${await getBearerToken()}`,
      };

      switch (requestType) {
        case REQUEST_TYPE.GET:
          response = await axios.get<T>(url, { headers: header });
          break;
        case REQUEST_TYPE.POST:
          response = await axios.post<T>(url, { headers: header });
          break;
          case REQUEST_TYPE.PUT:
            response = await axios.put<T>(url, { headers: header });
            break;
        default:
          throw new Error("Invalid request type");
      }
    }
  }

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
        scopes: [
          `api://fc97e872-bf3b-4531-82b0-8b85272982e2/Access`,
        ],
      });
    });

  if (authResult) {
    return authResult.accessToken;
  } else {
    throw new Error("Auth not successfull");
  }
};
