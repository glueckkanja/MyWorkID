import {
  authenticateRequest,
  getGraphBearerToken,
  sendAxiosRequest,
} from "./msal-service";
import {
  EApiFunctionTypes,
  REQUEST_TYPE,
  TDismissUserRiskResponse,
  TFunctionResult,
  TGenerateTapResponse,
  TGetRiskStateResponse,
  TVerifiedIdStatusResponse,
  TVerifyIdentityReponse,
  User,
  VerifiedIdAction,
} from "../types";
import axios from "axios";

const convertTFunctionResult = async <T>(
  axiosRequestPromise: Promise<T>,
  dataType: EApiFunctionTypes
): Promise<TFunctionResult<T>> => {
  try {
    const result = await axiosRequestPromise;
    return {
      status: "success",
      data: result,
      dataType: dataType,
    };
  } catch (error) {
    if (axios.isAxiosError(error)) {
      throw error;
    }
    return {
      status: "error",
      errorMessage: "Something went wrong on the client side.",
      dataType: dataType,
    };
  }
};

// Entra ID can take a few seconds until a freshly written verification is readable everywhere, so the
// request of an action that starts right after a verification may be rejected although the status
// already showed the verification.
const VERIFICATION_PROPAGATION_RETRY_INTERVAL_MILLISECONDS = 2000;
const VERIFICATION_PROPAGATION_MAXIMUM_ATTEMPTS = 8;

// The gate answers with 403 and the verification timestamps as problem details extensions. Other
// 403 responses (e.g. a missing role) do not carry them.
const isVerificationRejection = (error: unknown): boolean =>
  axios.isAxiosError(error) &&
  error.response?.status === 403 &&
  typeof error.response.data === "object" &&
  error.response.data !== null &&
  "lastVerifiedAt" in error.response.data;

const delay = (milliseconds: number) =>
  new Promise<void>((resolve) => setTimeout(resolve, milliseconds));

/**
 * Sends the request of an action that runs right after a verification. While the server still
 * rejects it for a missing verification, the request is repeated for a short while before the
 * rejection is reported.
 */
const requestAfterVerification = async <T>(
  request: () => Promise<T>
): Promise<T> => {
  for (let attempt = 1; ; attempt++) {
    try {
      return await request();
    } catch (error) {
      if (
        !isVerificationRejection(error) ||
        attempt >= VERIFICATION_PROPAGATION_MAXIMUM_ATTEMPTS
      ) {
        throw error;
      }
      await delay(VERIFICATION_PROPAGATION_RETRY_INTERVAL_MILLISECONDS);
    }
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

/**
 * @param startedRightAfterVerification True when the user just verified their identity for this
 * action, so a rejection for a missing verification is retried for a short while.
 */
export const dismissUserRisk = async (
  startedRightAfterVerification = false
): Promise<TFunctionResult<TDismissUserRiskResponse>> => {
  const sendRequest = () =>
    authenticateRequest<TDismissUserRiskResponse>(
      `${backendApiUrl}/me/riskstate/dismiss`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.DISMISS_USER_RISK
    );
  return convertTFunctionResult<TDismissUserRiskResponse>(
    startedRightAfterVerification
      ? requestAfterVerification(sendRequest)
      : sendRequest(),
    EApiFunctionTypes.DISMISS_USER_RISK
  );
};

/**
 * @param startedRightAfterVerification True when the user just verified their identity for this
 * action, so a rejection for a missing verification is retried for a short while.
 */
export const generateTAP = async (
  startedRightAfterVerification = false
): Promise<TFunctionResult<TGenerateTapResponse>> => {
  const sendRequest = () =>
    authenticateRequest<TGenerateTapResponse>(
      `${backendApiUrl}/me/generatetap`,
      REQUEST_TYPE.PUT,
      EApiFunctionTypes.CREATE_TAP
    );
  return convertTFunctionResult<TGenerateTapResponse>(
    startedRightAfterVerification
      ? requestAfterVerification(sendRequest)
      : sendRequest(),
    EApiFunctionTypes.CREATE_TAP
  );
};

export const revokeTemporaryAccessPass = async (
  temporaryAccessPassId: string
): Promise<TFunctionResult<unknown>> => {
  return convertTFunctionResult(
    authenticateRequest(
      `${backendApiUrl}/me/tap/${encodeURIComponent(temporaryAccessPassId)}`,
      REQUEST_TYPE.DELETE,
      EApiFunctionTypes.REVOKE_TAP
    ),
    EApiFunctionTypes.REVOKE_TAP
  );
};

/**
 * Starts an identity verification. When `action` is given, the verification is bound to it: if every
 * action requires its own verification, it only unlocks that action.
 */
export const verifyIdentity = async (
  action?: VerifiedIdAction,
): Promise<TFunctionResult<TVerifyIdentityReponse>> => {
  const query = action ? `?action=${encodeURIComponent(action)}` : "";
  return convertTFunctionResult(
    authenticateRequest<TVerifyIdentityReponse>(
      `${backendApiUrl}/me/verifiedId/verify${query}`,
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
    authenticateRequest(
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
    authenticateRequest(
      `${backendApiUrl}/me/resetPassword/checkClaim`,
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

/**
 * Discards a verification that was started for `action` without being used, so it cannot be used
 * later. The server ignores the request unless the current verification is bound to `action`.
 */
export const discardVerification = async (
  action: VerifiedIdAction,
): Promise<void> => {
  await authenticateRequest(
    `${backendApiUrl}/me/verifiedId/discard?action=${encodeURIComponent(action)}`,
    REQUEST_TYPE.POST
  );
};

export const getVerifiedIdStatus =
  async (): Promise<TVerifiedIdStatusResponse> => {
    return await authenticateRequest(
      `${backendApiUrl}/me/verifiedId/status`,
      REQUEST_TYPE.GET
    );
  };
