import {
  authenticateRequest,
  getGraphBearerToken,
  sendAxiosRequest,
} from "./msal-service";
import {
  EApiFunctionTypes,
  REQUEST_TYPE,
  TFunctionResult,
  TGenerateTapResponse,
  TGetRiskStateResponse,
  TVerifyIdentityReponse,
  User,
} from "../types";
import axios from "axios";

const convertTFunctionResult = async <T>(
  functionResult: T,
  dataType: EApiFunctionTypes
): Promise<TFunctionResult<T>> => {
  try {
    const result = functionResult;
    return {
      status: "success",
      data: result,
      dataType: dataType,
    };
  } catch (error) {
    if (!axios.isAxiosError(error)) {
      throw error;
    }
    return {
      status: "error",
      errorMessage: error.message,
      dataType: dataType,
    };
  }
};

const backendApiUrl: string = `${window.location.protocol}//${window.location.host}/api`;

export const getUser = async (): Promise<User> => {
  const token = await getGraphBearerToken();
  return (
    await sendAxiosRequest<User>(
      "https://graph.microsoft.com/v1.0/me",
      REQUEST_TYPE.GET,
      token
    )
  ).data;
};
export const getUserImage = async (): Promise<Blob> => {
  const token = await getGraphBearerToken();
  return (
    await axios.get(
      "https://graph.microsoft.com/v1.0/me/photos/120x120/$value",
      { headers: { Authorization: `Bearer ${token}` }, responseType: "blob" }
    )
  ).data;
};

export const dismissUserRisk = async (): Promise<TFunctionResult<unknown>> => {
  return convertTFunctionResult(
    await authenticateRequest(
      `${backendApiUrl}/me/riskstate/dismiss`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.DISMISS_USER_RISK
    ),
    EApiFunctionTypes.DISMISS_USER_RISK
  );
};
export const generateTAP = async (): Promise<
  TFunctionResult<TGenerateTapResponse>
> => {
  return convertTFunctionResult(
    await authenticateRequest<TGenerateTapResponse>(
      `${backendApiUrl}/me/generatetap`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.CREATE_TAP
    ),
    EApiFunctionTypes.CREATE_TAP
  );
};

export const verifyIdentity = async (): Promise<
  TFunctionResult<TVerifyIdentityReponse>
> => {
  return convertTFunctionResult(
    await authenticateRequest<TVerifyIdentityReponse>(
      `${backendApiUrl}/me/verifiedId/verify`,
      REQUEST_TYPE.POST,
      EApiFunctionTypes.VALIDATE_IDENTITY
    ),
    EApiFunctionTypes.VALIDATE_IDENTITY
  );
};

export const callResetPassword = async (
  newPassword: string
): Promise<TFunctionResult<TGenerateTapResponse>> => {
  return convertTFunctionResult(
    await authenticateRequest(
      `${backendApiUrl}/me/resetPassword`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.PASSWORD_RESET,
      {
        newPassword,
      }
    ),
    EApiFunctionTypes.PASSWORD_RESET
  );
};

export const checkResetPasswordClaim = async (): Promise<
  TFunctionResult<TGenerateTapResponse>
> => {
  return convertTFunctionResult(
    await authenticateRequest(
      `${backendApiUrl}/api/resetpasswordcontroller/checkClaim`,
      REQUEST_TYPE.GET,
      EApiFunctionTypes.PASSWORD_RESET
    ),
    EApiFunctionTypes.PASSWORD_RESET
  );
};

export const getUserRiskState = async (): Promise<TGetRiskStateResponse> => {
  return await authenticateRequest(
    `${backendApiUrl}/me/riskstate`,
    REQUEST_TYPE.GET
  );
};
