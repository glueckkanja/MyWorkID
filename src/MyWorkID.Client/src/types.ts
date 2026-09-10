import { ReactNode, ComponentPropsWithoutRef } from "react";
import type { ToastActionElement, ToastProps } from "@/components/ui/toast";

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

export enum RiskLevel {
  High = "high",
  Medium = "medium",
  Low = "low",
  None = "none",
  Unknown = "unknown",
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

export type UserProfile = {
  user: User | undefined;
  userImage: string | undefined;
  riskLoading: boolean;
  riskLevel: RiskLevel;
  riskLabel: string;
  maxDismissibleRiskLevel: RiskLevel;
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
  maxDismissibleRiskLevel: string;
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
