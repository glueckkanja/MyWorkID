export enum REQUEST_TYPE {
  GET,
  POST,
  PUT
}

export type TWWWAuthenticateHeader = {
  claims: string;
};

export type TFrontendOptions = {
  frontendClientId: string,
  tenantId: string,
  backendClientId: string,
}