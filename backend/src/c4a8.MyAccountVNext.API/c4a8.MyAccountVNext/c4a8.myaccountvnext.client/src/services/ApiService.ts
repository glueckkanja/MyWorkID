import {
  authenticateRequest,
  getGraphBearerToken,
  sendAxiosRequest,
} from "./MsalService";
import {
  EApiFunctionTypes,
  REQUEST_TYPE,
  TFrontendOptions,
  TFunctionResult,
  TGenerateTapResponse,
  TGetRiskStateResponse,
  User,
} from "../types";
import axios from "axios";

const settings: TFrontendOptions = {
    backendApiUrl: "https://localhost:7266",
    frontendClientId: "7e19e859-ddf5-402c-8115-8ee20c575b4a",
    tenantId: "a9ae459a-6068-4a03-915a-7031507edbc1",
    backendClientId: "b808da2f-1ae8-4f02-a4e9-320fd6a1dc6b"
};

const convertTFunctionResult = async <T>(
  promise: T,
  dataType: EApiFunctionTypes
): Promise<TFunctionResult<T>> => {
  try {
    const result = await promise;
    return {
      status: "success",
      data: result,
      dataType: dataType,
    };
  } catch (error: any) {
    return {
      status: "error",
      errorMessage: error.message,
      dataType: dataType,
    };
  }
};

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

export const dismissUserRisk = async (): Promise<TFunctionResult<any>> => {
  return convertTFunctionResult(
    await authenticateRequest(
      `${settings.backendApiUrl}/DismissUserRisk`,
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
      `${settings.backendApiUrl}/GenerateTap`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.CREATE_TAP
    ),
    EApiFunctionTypes.CREATE_TAP
  );
};

export const getUserRiskState = async (): Promise<TGetRiskStateResponse> => {
  return await authenticateRequest(
    `${settings.backendApiUrl}/api/users/me/riskstate`,
    REQUEST_TYPE.GET
  );
};
