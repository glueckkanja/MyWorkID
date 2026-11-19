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
  /** When set, the blocked notice offers to start the identity verification. */
  onVerifyIdentity?: () => void;
  /** False hides the blocked reason, e.g. when the verification button already explains what to do. */
  showBlockedReason?: boolean;
  /** True while a just completed verification is being confirmed; the panel shows a spinner. */
  waitingForVerification?: boolean;
  /** Called after a Temporary Access Pass was generated. */
  onCompleted?: () => void;
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
  /** True while a just completed verification is being confirmed; the panel shows a spinner. */
  waitingForVerification?: boolean;
  /** When set, the disabled dismiss button is replaced by a button that starts the identity verification. */
  onVerifyIdentity?: () => void;
};

export type PasswordResetPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
  /** When set, the action is blocked (e.g. identity not verified) and this reason is shown instead of the form. */
  blockedReason?: string;
  /** When set, the blocked notice offers to start the identity verification. */
  onVerifyIdentity?: () => void;
  /** False hides the blocked reason, e.g. when the verification button already explains what to do. */
  showBlockedReason?: boolean;
  /** True while a just completed verification is being confirmed; the panel shows a spinner. */
  waitingForVerification?: boolean;
  /** Called after the password was changed. */
  onCompleted?: () => void;
};

export type ValidateIdentityPanelProps = {
  open: boolean;
  onClose: () => void;
  /** Called after the server confirmed a successful verification, with the action it was started for. */
  onVerified?: (verificationAction?: VerifiedIdAction) => void;
  /** The action the verification is started for, if it was started from an action panel. */
  verificationAction?: VerifiedIdAction;
  /** Current verification requirement/state, used to explain the validity window to the user. */
  verifiedIdStatus?: VerifiedIdStatus;
};

export type VerifyState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "ready"; qrCodeBase64: string }
  // The QR code was scanned; the face check continues on the mobile device.
  | { status: "scanned" };

export type ActionCardProps = {
  icon: ReactNode;
  title: string;
  description: string;
  highlighted?: boolean;
  onClick: () => void;
  disabled?: boolean;
  disabledReason?: string;
};

export type BlockedActionNoticeProps = {
  /** Explains why the action is currently unavailable. */
  reason: string;
  /** True while a just completed verification is being confirmed; a spinner replaces the notice. */
  waiting?: boolean;
  /** Label of the button that would normally trigger the action. */
  actionLabel: string;
  /** Icon of that button, if the action button has one. */
  actionIcon?: ReactNode;
  /** Style of that button. Defaults to the primary style. */
  actionVariant?: "primary" | "secondary";
  /** When set, the action button starts the identity verification instead of being disabled. */
  onVerifyIdentity?: () => void;
  /** False hides the reason, e.g. when the verification button already explains what to do. */
  showReason?: boolean;
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
 * A sensitive action a verification can be bound to. The names match the server's `VerifiedIdAction`.
 */
export type VerifiedIdAction =
  | "DismissUserRisk"
  | "GenerateTemporaryAccessPass"
  | "ResetPassword";

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
  /** True if every sensitive action requires its own verification, because no validity window is configured. */
  requiresVerificationPerAction: boolean;
  /** True if the user has a verification that is still within the validity window (and, per action, not used yet). */
  isVerified: boolean;
  /** The action the verification is bound to. When every action requires its own verification, only this action is unlocked. */
  verifiedForAction: VerifiedIdAction | undefined;
  /** True while a just completed verification is not visible in the status yet. */
  confirmingVerification: boolean;
  lastVerifiedAt: Date | undefined;
  validUntil: Date | undefined;
  validityInMinutes: number;
  refresh: () => void;
  /** Reloads the status repeatedly until the fresh verification is visible (the directory can lag behind). */
  refreshAfterVerification: () => void;
  /** Call after a sensitive action succeeded. When every action requires its own verification, the current one no longer counts. */
  markVerificationAsUsed: () => void;
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

export type TDismissUserRiskResponse = {
  sessionsRevoked: boolean;
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
  requiresVerificationPerAction?: boolean;
  isVerified: boolean;
  verifiedForAction?: VerifiedIdAction | null;
  lastVerifiedAt?: string | null;
  validUntil?: string | null;
  serverTime?: string | null;
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
