import {
  authenticateRequest,
  getGraphBearerToken,
  sendAxiosRequest,
} from "./MsalService";
import {
  EApiFunctionTypes,
  REQUEST_TYPE,
  TFunctionResult,
  TGenerateTapResponse,
  User,
} from "../types";
import axios from "axios";

const convertTFunctionResult = async <T>(
  promise: T,
  dataType: EApiFunctionTypes
): Promise<TFunctionResult<T>> => {
  try {
    const result = await promise;
    return {
      status: "success",
      data: result,
      dataType: EApiFunctionTypes.DISMISS_USER_RISK,
    };
  } catch (error: any) {
    return {
      status: "error",
      errorMessage: error.message,
      dataType: EApiFunctionTypes.DISMISS_USER_RISK,
    };
  }
};

export const getUser = async (): Promise<User> => {
  var token = await getGraphBearerToken();
  return (
    await sendAxiosRequest<User>(
      "https://graph.microsoft.com/v1.0/me",
      REQUEST_TYPE.GET,
      token
    )
  ).data;
};
export const getUserImage = async (): Promise<Blob> => {
  var token = await getGraphBearerToken();
  return (await axios.get(
    "https://graph.microsoft.com/v1.0/me/photos/120x120/$value",
    { headers: { Authorization: `Bearer ${token}` }, responseType: "blob" }
  )).data;
};

export const dismissUserRisk = async (): Promise<TFunctionResult<any>> => {
  return convertTFunctionResult(
    await authenticateRequest(
      `${window.settings.backendApiUrl}/DismissUserRisk`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.DISMISS_USER_RISK
    ),
    EApiFunctionTypes.DISMISS_USER_RISK
  );
};
export const generateTAP = async (): Promise<TFunctionResult<TGenerateTapResponse>> => {
  return convertTFunctionResult(
    await authenticateRequest<TGenerateTapResponse>(
      `${window.settings.backendApiUrl}/GenerateTap`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.CREATE_TAP
    ),
    EApiFunctionTypes.CREATE_TAP
  );
};
