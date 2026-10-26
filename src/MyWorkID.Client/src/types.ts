import { ReactNode, ComponentPropsWithoutRef } from "react";
import type { ToastActionElement, ToastProps } from "@/components/ui/toast";
import type { RiskLevel } from "@/lib/risk-level";

export type Theme = "dark" | "light" | "system";
export type ResolvedTheme = "dark" | "light";

export type ThemeProviderState = {
  theme: Theme;
  resolvedTheme: ResolvedTheme;
  setTheme: (theme: Theme) => void;
};

export type ThemeProviderProps = {
  children: ReactNode;
  defaultTheme?: Theme;
  storageKey?: string;
};

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
  /** When set, the action is blocked (e.g. identity not verified) and this reason is shown instead of the form. */
  blockedReason?: string;
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
  canDismiss: boolean;
  dismissDisabledReason?: string;
};

export type PasswordResetPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
  /** When set, the action is blocked (e.g. identity not verified) and this reason is shown instead of the form. */
  blockedReason?: string;
};

export type ValidateIdentityPanelProps = {
  open: boolean;
  onClose: () => void;
  /** Called after the server confirmed a successful verification. */
  onVerified?: () => void;
  /** Current verification requirement/state, used to explain the validity window to the user. */
  verifiedIdStatus?: VerifiedIdStatus;
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
  disabled?: boolean;
  disabledReason?: string;
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

export type AppProps = {
  maxDismissibleRiskLevel: RiskLevel;
};

export type UserProfile = {
  user: User | undefined;
  userImage: string | undefined;
  riskLoading: boolean;
  riskLevel: RiskLevel;
  riskLabel: string;
  refreshRiskState: () => void;
};

/**
 * Verification state of the signed-in user with regard to the optional
 * "require recent Verified ID" feature.
 */
export type VerifiedIdStatus = {
  /** True while the first status request is in flight. */
  loading: boolean;
  /** True if the status could not be loaded. The server still enforces the requirement. */
  error: boolean;
  /** True if sensitive actions require a recent verification. */
  required: boolean;
  /** True if the user has a verification that is still within the validity window. */
  isVerified: boolean;
  lastVerifiedAt: Date | undefined;
  validUntil: Date | undefined;
  validityInMinutes: number;
  refresh: () => void;
};

export type TWWWAuthenticateHeader = {
  claims: string;
};

export type TFrontendOptions = {
  frontendClientId: string;
  tenantId: string;
  backendClientId: string;
  maxDismissibleRiskLevel: string;
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
  maxDismissibleRiskLevel: string;
};

export type TVerifiedIdStatusResponse = {
  required: boolean;
  validityInMinutes: number;
  isVerified: boolean;
  lastVerifiedAt?: string | null;
  validUntil?: string | null;
};

export type ToasterToast = ToastProps & {
  id: string;
  title?: ReactNode;
  description?: ReactNode;
  action?: ToastActionElement;
  correlationId?: string;
};

export type ToastActionType = {
  ADD_TOAST: "ADD_TOAST";
  UPDATE_TOAST: "UPDATE_TOAST";
  DISMISS_TOAST: "DISMISS_TOAST";
  REMOVE_TOAST: "REMOVE_TOAST";
};

export type ToastAction =
  | {
      type: ToastActionType["ADD_TOAST"];
      toast: ToasterToast;
    }
  | {
      type: ToastActionType["UPDATE_TOAST"];
      toast: Pick<ToasterToast, "id"> & Partial<Omit<ToasterToast, "id">>;
    }
  | {
      type: ToastActionType["DISMISS_TOAST"];
      toastId?: ToasterToast["id"];
    }
  | {
      type: ToastActionType["REMOVE_TOAST"];
      toastId?: ToasterToast["id"];
    };

export type ToastState = {
  toasts: ToasterToast[];
};
