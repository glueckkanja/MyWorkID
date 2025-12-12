export enum REQUEST_TYPE {
  GET,
  POST,
  PUT,
}

export type TWWWAuthenticateHeader = {
  claims: string;
};

export type TFrontendOptions = {
  frontendClientId: string;
  tenantId: string;
  backendClientId: string;
  customCssUrl?: string;
};

export type TFunctionResult<T = undefined> = {
  status: "success" | "error" | "pending";
  errorMessage?: string;
  data?: T;
  dataType: EApiFunctionTypes;
};

export enum EApiFunctionTypes {
  DISMISS_USER_RISK = "dismissUserRisk",
  CREATE_TAP = "createTap",
  PASSWORD_RESET = "passwordReset",
  VALIDATE_IDENTITY = "validateIdentity",
  UNKNOWN = "unknown",
}

export type TFunctionProps = {
  comingFromRedirect: boolean;
};

export type ActionResultProps<T> = {
  result?: TFunctionResult<T>;
};

export type User = {
  displayName: string;
};

export type TGenerateTapResponse = {
  temporaryAccessPassword: string;
};

export type TVerifyIdentityReponse = {
  requestId: string;
  url: string;
  expiry: number;
  qrCode: string;
};

export type TGetRiskStateResponse = {
  riskState: string;
  riskLevel?: string;
};
