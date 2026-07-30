import { ReactNode, ComponentPropsWithoutRef } from "react";

export enum REQUEST_TYPE {
  GET,
  POST,
  PUT,
  DELETE,
}

export type CreateTapPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
};

export type TapState =
  | { status: "empty" }
  | { status: "loading" }
  | { status: "revoking" }
  | { status: "ready"; id: string; password: string };

export type DismissUserRiskPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
  onDismissed?: () => void;
  riskLevel: RiskLevel;
  riskLabel: string;
};

export type PasswordResetPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
};

export type ValidateIdentityPanelProps = {
  open: boolean;
  onClose: () => void;
};

export type VerifyState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "ready"; qrCodeBase64: string };

export type ActionCardProps = {
  icon: ReactNode;
  title: string;
  description: string;
  highlighted?: boolean;
  onClick: () => void;
};

export type PanelKey =
  | "password"
  | "tap"
  | "revokeTap"
  | "dismiss"
  | "validate"
  | null;

export type SvgIconProps = ComponentPropsWithoutRef<"svg">;

export type PanelProps = {
  open: boolean;
  title: string;
  subtitle?: string;
  onClose: () => void;
  children: ReactNode;
};

export type RiskLevel = "high" | "medium" | "low" | "none" | "unknown";

export type UserProfile = {
  user: User | undefined;
  userImage: string | undefined;
  riskLoading: boolean;
  riskLevel: RiskLevel;
  riskLabel: string;
  refreshRiskState: () => void;
};

export type TWWWAuthenticateHeader = {
  claims: string;
};

export type TFrontendOptions = {
  frontendClientId: string;
  tenantId: string;
  backendClientId: string;
  customCssUrl?: string;
  appTitle?: string;
  faviconUrl?: string;
  helpUrl?: string;
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
  REVOKE_TAP = "revokeTap",
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
  mail?: string;
  userPrincipalName?: string;
};

export type TGenerateTapResponse = {
  temporaryAccessPassId: string;
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
