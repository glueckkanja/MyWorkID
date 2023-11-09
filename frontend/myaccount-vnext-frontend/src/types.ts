export enum REQUEST_TYPE {
  GET,
  POST,
  PUT
}

export type TWWWAuthenticateHeader = {
  claims: string;
};

export type TFrontendOptions = {
  backendApiUrl: string,
  frontendClientId: string,
  tenantId: string,
  backendClientId: string,
}

export type TFunctionResult<T> = {
  status: "success" | "error" | "pending";
  errorMessage?: string;
  data?: T;
  dataType: EApiFunctionTypes;
}

export enum EApiFunctionTypes {
  DISMISS_USER_RISK = "dismissUserRisk",
  CREATE_TAP = "createTap",
  PASSWORD_RESET = "passwordReset",
  UNKNOWN = "unknown"
};

export type ActionResultProps<T> = {
  result?: TFunctionResult<T>
}

export type User = {
  displayName: string;
}

export type TGenerateTapResponse = {
  temporaryAccessPassword: string;
}

export type TGetRiskStateResponse = {
  riskState: string;
}