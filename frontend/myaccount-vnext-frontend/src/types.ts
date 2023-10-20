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