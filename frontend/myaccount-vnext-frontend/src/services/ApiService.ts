import { authenticateRequest } from "./MsalService";
import { EApiFunctionTypes, REQUEST_TYPE, TFunctionResult } from "../types";

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

export const dismissUserRisk = async (): Promise<TFunctionResult<any>> => {
  return convertTFunctionResult(
    authenticateRequest(
      `${window.settings.backendApiUrl}/DismissUserRisk`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.DISMISS_USER_RISK
    ),
    EApiFunctionTypes.DISMISS_USER_RISK
  );
};
export const createTAP = async (): Promise<TFunctionResult<any>> => {
  return convertTFunctionResult(
    authenticateRequest(
      `${window.settings.backendApiUrl}/CreateTAP`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.CREATE_TAP
    ),
    EApiFunctionTypes.CREATE_TAP
  );
};
