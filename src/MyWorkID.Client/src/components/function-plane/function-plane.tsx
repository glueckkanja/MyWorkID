import { useCallback, useEffect, useState } from "react";

// Import services
import {
  getPendingAction,
  handleRedirectPromise,
} from "../../services/msal-service";
import { useSignedInUser } from "../../contexts/signed-in-user-provider";
import { Role } from "../../services/roles-service";
import {
  EApiFunctionTypes,
  PanelKey,
  VerifiedIdAction,
  VerifiedIdStatus,
} from "../../types";
import type { AppProps } from "../../types";

// Import app state and utilities
import { useUserProfile } from "@/hooks/use-user-profile";
import { useVerifiedIdStatus } from "@/hooks/use-verified-id-status";
import {
  canDismissRiskLevel,
  getRiskLabelFromLevel,
  RiskLevel,
} from "@/lib/risk-level";

// Import feature components
import { ActionCard } from "./action-card";
import { CreateTapPanel } from "./function-plane-components/create-tap";
import { DismissUserRiskPanel } from "./function-plane-components/dismiss-userisk";
import { PasswordResetPanel } from "./function-plane-components/password-reset";
import { ValidateIdentityPanel } from "./function-plane-components/validate-identity";
import { UserDisplay } from "./user-display";

// Import icons
import { CircleCheckIcon } from "@/components/ui/icons/circle-check-icon";
import { IdentityVerificationIcon } from "@/components/ui/icons/identity-verification-icon";
import { PasswordLockIcon } from "@/components/ui/icons/password-lock-icon";
import { TemporaryAccessPassIcon } from "@/components/ui/icons/temporary-access-pass-icon";

const PANEL_BY_ACTION: Record<
  EApiFunctionTypes,
  Exclude<PanelKey, null> | null
> = {
  [EApiFunctionTypes.PASSWORD_RESET]: "password",
  [EApiFunctionTypes.CREATE_TAP]: "tap",
  [EApiFunctionTypes.REVOKE_TAP]: "revokeTap",
  [EApiFunctionTypes.DISMISS_USER_RISK]: "dismiss",
  [EApiFunctionTypes.VALIDATE_IDENTITY]: "validate",
  [EApiFunctionTypes.UNKNOWN]: null,
};

const VERIFICATION_REQUIRED_REASON =
  "Validate your identity first to unlock this action.";
const VERIFICATION_EXPIRED_REASON =
  "Your identity verification has expired. Validate your identity again to unlock this action.";
const VERIFICATION_REQUIRED_PER_ACTION_REASON =
  "For your security, this action requires a new identity verification every time. Verify your identity to continue.";
const VERIFICATION_CONFIRMING_REASON =
  "Confirming your identity verification. This action becomes available in a moment.";
const VERIFICATION_STATUS_LOADING_REASON =
  "Loading your account status. This action becomes available once loading has finished.";
const VERIFICATION_STATUS_ERROR_REASON =
  "Your identity verification status could not be loaded. Please try again later.";
const VERIFICATION_NOT_POSSIBLE_REASON =
  "Identity verification is required for this action, but you don't have permission to validate your identity. Please contact your administrator.";

// Action a verification started from a panel is bound to.
const VERIFIED_ID_ACTION_BY_PANEL: Partial<
  Record<Exclude<PanelKey, null>, VerifiedIdAction>
> = {
  password: "ResetPassword",
  tap: "GenerateTemporaryAccessPass",
  dismiss: "DismissUserRisk",
};

type ActionVerificationState = {
  /** True while the action must not run, because the verification state is unknown or insufficient. */
  blocked: boolean;
  /** Explains why the action is blocked. */
  blockedReason: string | undefined;
  /** True if the user can start a verification for this action from its panel. */
  canVerifyIdentity: boolean;
};

/**
 * When the optional "require recent Verified ID" feature is enabled, sensitive actions are blocked
 * until the user completed a face check within the configured validity window. Without a validity
 * window, every action requires its own face check, bound to that action.
 * Until the status is known, actions stay blocked so that neither a click nor an automatic resume
 * after an authentication redirect can run against an unknown requirement.
 */
const getActionVerificationState = (
  action: VerifiedIdAction,
  verifiedIdStatus: VerifiedIdStatus,
  canValidateIdentity: boolean,
): ActionVerificationState => {
  if (verifiedIdStatus.loading) {
    return {
      blocked: true,
      blockedReason: VERIFICATION_STATUS_LOADING_REASON,
      canVerifyIdentity: false,
    };
  }
  if (verifiedIdStatus.error) {
    return {
      blocked: true,
      blockedReason: VERIFICATION_STATUS_ERROR_REASON,
      canVerifyIdentity: false,
    };
  }

  const isVerifiedForAction =
    verifiedIdStatus.isVerified &&
    (!verifiedIdStatus.requiresVerificationPerAction ||
      verifiedIdStatus.verifiedForAction === action);
  if (!verifiedIdStatus.required || isVerifiedForAction) {
    return {
      blocked: false,
      blockedReason: undefined,
      canVerifyIdentity: false,
    };
  }

  let blockedReason: string;
  if (!canValidateIdentity) {
    blockedReason = VERIFICATION_NOT_POSSIBLE_REASON;
  } else if (verifiedIdStatus.confirmingVerification) {
    blockedReason = VERIFICATION_CONFIRMING_REASON;
  } else if (verifiedIdStatus.requiresVerificationPerAction) {
    blockedReason = VERIFICATION_REQUIRED_PER_ACTION_REASON;
  } else if (verifiedIdStatus.lastVerifiedAt !== undefined) {
    blockedReason = VERIFICATION_EXPIRED_REASON;
  } else {
    blockedReason = VERIFICATION_REQUIRED_REASON;
  }
  return {
    blocked: true,
    blockedReason,
    canVerifyIdentity:
      canValidateIdentity && !verifiedIdStatus.confirmingVerification,
  };
};

const DISMISS_ICON = (
  <CircleCheckIcon width={20} height={20} strokeWidth={1.8} />
);
const PASSWORD_ICON = <PasswordLockIcon />;
const TEMPORARY_ACCESS_PASS_ICON = <TemporaryAccessPassIcon />;
const IDENTITY_VERIFICATION_ICON = <IdentityVerificationIcon />;

const FunctionPlane = ({ maxDismissibleRiskLevel }: AppProps) => {
  const [activePanel, setActivePanel] = useState<PanelKey>(null);
  const [redirectAction, setRedirectAction] = useState<EApiFunctionTypes>();
  const signedInUserInfo = useSignedInUser();
  const profile = useUserProfile();
  const verifiedIdStatus = useVerifiedIdStatus();
  const { refreshAfterVerification, markVerificationAsUsed } = verifiedIdStatus;
  const { refreshRiskState } = profile;
  // Action panel to reopen once the identity verification that was started from it succeeded.
  const [panelAwaitingVerification, setPanelAwaitingVerification] =
    useState<PanelKey>(null);

  const openPasswordPanel = useCallback(() => setActivePanel("password"), []);
  const openTemporaryAccessPassPanel = useCallback(
    () => setActivePanel("tap"),
    [],
  );
  const openDismissPanel = useCallback(() => setActivePanel("dismiss"), []);
  const openValidatePanel = useCallback(() => {
    setPanelAwaitingVerification(null);
    setActivePanel("validate");
  }, []);

  const verifyIdentityForPanel = useCallback((panel: PanelKey) => {
    setPanelAwaitingVerification(panel);
    setActivePanel("validate");
  }, []);
  const verifyIdentityForPasswordReset = useCallback(
    () => verifyIdentityForPanel("password"),
    [verifyIdentityForPanel],
  );
  const verifyIdentityForTemporaryAccessPass = useCallback(
    () => verifyIdentityForPanel("tap"),
    [verifyIdentityForPanel],
  );
  const verifyIdentityForDismissRisk = useCallback(
    () => verifyIdentityForPanel("dismiss"),
    [verifyIdentityForPanel],
  );

  const handleVerified = useCallback(() => {
    refreshAfterVerification();
    if (panelAwaitingVerification) {
      setActivePanel(panelAwaitingVerification);
      setPanelAwaitingVerification(null);
    }
  }, [refreshAfterVerification, panelAwaitingVerification]);

  const handleRiskDismissed = useCallback(() => {
    refreshRiskState();
    markVerificationAsUsed();
  }, [refreshRiskState, markVerificationAsUsed]);

  useEffect(() => {
    handleRedirectPromise()
      .then((authenticationResult) => {
        if (!authenticationResult) {
          return;
        }
        try {
          const action = getPendingAction(authenticationResult);
          setRedirectAction(action);
          const panel = PANEL_BY_ACTION[action];
          if (panel) {
            setActivePanel(panel);
          }
        } catch (error) {
          // ignore - no valid pending action
          if (import.meta.env.DEV) {
            // Log only for debugging purposes in dev mode
            console.debug(
              "No valid pending action found in redirect result",
              error,
            );
          }
        }
      })
      .catch((error) => {
        if (import.meta.env.DEV) {
          console.debug("handleRedirectPromise failed", error);
        }
      });
  }, []);

  const hasRole = (role: string) => !!signedInUserInfo?.roles?.includes(role);

  const canResetPassword = hasRole(Role.ALLOW_CHANGE_PASSWORD);
  const canCreateTap = hasRole(Role.ALLOW_CREATE_TAP);
  const canDismissRisk = hasRole(Role.ALLOW_DISMISS_USER_RISK);
  const canValidateIdentity = hasRole(Role.ALLOW_VALIDATE_IDENTITY);

  const passwordResetVerification = getActionVerificationState(
    "ResetPassword",
    verifiedIdStatus,
    canValidateIdentity,
  );
  const temporaryAccessPassVerification = getActionVerificationState(
    "GenerateTemporaryAccessPass",
    verifiedIdStatus,
    canValidateIdentity,
  );
  const dismissRiskVerification = getActionVerificationState(
    "DismissUserRisk",
    verifiedIdStatus,
    canValidateIdentity,
  );

  const dismissAllowedForCurrentLevel =
    profile.riskLoading ||
    canDismissRiskLevel(profile.riskLevel, maxDismissibleRiskLevel);
  const dismissDisabledReason = dismissAllowedForCurrentLevel
    ? dismissRiskVerification.blockedReason
    : `Your ${getRiskLabelFromLevel(profile.riskLevel)} risk level cannot be dismissed via self-service. Please contact your administrator.`;

  const showRecommendedDismiss =
    canDismissRisk &&
    !dismissRiskVerification.blocked &&
    !profile.riskLoading &&
    dismissAllowedForCurrentLevel &&
    (profile.riskLevel === RiskLevel.High ||
      profile.riskLevel === RiskLevel.Medium);

  const closePanel = useCallback(() => {
    setActivePanel(null);
    setRedirectAction(undefined);
  }, []);

  return (
    <main className="app-main">
      <UserDisplay profile={profile} />

      {showRecommendedDismiss && (
        <>
          <div className="section-label">Recommended</div>
          <div className="actions-list">
            <ActionCard
              icon={DISMISS_ICON}
              title="Dismiss Risk"
              description="Resolve your risk status and revoke all your sessions to regain full access"
              highlighted
              onClick={openDismissPanel}
            />
          </div>
        </>
      )}

      <div className="section-label">All Actions</div>
      <div className="actions-list">
        {canResetPassword && (
          <ActionCard
            icon={PASSWORD_ICON}
            title="Reset Password"
            description="Set a new password for your account"
            onClick={openPasswordPanel}
          />
        )}
        {canCreateTap && (
          <ActionCard
            icon={TEMPORARY_ACCESS_PASS_ICON}
            title="Temporary Access Pass"
            description="Get a one-time code to sign in or set up a new device"
            onClick={openTemporaryAccessPassPanel}
          />
        )}
        {canDismissRisk && !showRecommendedDismiss && (
          <ActionCard
            icon={DISMISS_ICON}
            title="Dismiss User Risk"
            description="Clear your account risk status and revoke all your sessions"
            onClick={openDismissPanel}
          />
        )}
        {canValidateIdentity && (
          <ActionCard
            icon={IDENTITY_VERIFICATION_ICON}
            title="Validate Identity"
            description="Verify who you are using Face Check"
            onClick={openValidatePanel}
          />
        )}
      </div>

      {canResetPassword && (
        <PasswordResetPanel
          open={activePanel === "password"}
          onClose={closePanel}
          comingFromRedirect={
            !passwordResetVerification.blocked &&
            redirectAction === EApiFunctionTypes.PASSWORD_RESET
          }
          blockedReason={passwordResetVerification.blockedReason}
          onVerifyIdentity={
            passwordResetVerification.canVerifyIdentity
              ? verifyIdentityForPasswordReset
              : undefined
          }
          onCompleted={markVerificationAsUsed}
        />
      )}
      {canCreateTap && (
        <CreateTapPanel
          open={activePanel === "tap" || activePanel === "revokeTap"}
          onClose={closePanel}
          comingFromRedirect={
            !temporaryAccessPassVerification.blocked &&
            redirectAction === EApiFunctionTypes.CREATE_TAP
          }
          blockedReason={temporaryAccessPassVerification.blockedReason}
          onVerifyIdentity={
            temporaryAccessPassVerification.canVerifyIdentity
              ? verifyIdentityForTemporaryAccessPass
              : undefined
          }
          onCompleted={markVerificationAsUsed}
        />
      )}
      {canDismissRisk && (
        <DismissUserRiskPanel
          open={activePanel === "dismiss"}
          onClose={closePanel}
          comingFromRedirect={
            !dismissRiskVerification.blocked &&
            redirectAction === EApiFunctionTypes.DISMISS_USER_RISK
          }
          onDismissed={handleRiskDismissed}
          riskLevel={profile.riskLevel}
          riskLabel={profile.riskLabel}
          canDismiss={dismissAllowedForCurrentLevel && !dismissRiskVerification.blocked}
          dismissDisabledReason={dismissDisabledReason}
          onVerifyIdentity={
            dismissAllowedForCurrentLevel && dismissRiskVerification.canVerifyIdentity
              ? verifyIdentityForDismissRisk
              : undefined
          }
        />
      )}
      {canValidateIdentity && (
        <ValidateIdentityPanel
          open={activePanel === "validate"}
          onClose={closePanel}
          onVerified={handleVerified}
          verificationAction={
            panelAwaitingVerification
              ? VERIFIED_ID_ACTION_BY_PANEL[panelAwaitingVerification]
              : undefined
          }
          verifiedIdStatus={verifiedIdStatus}
        />
      )}
    </main>
  );
};

export default FunctionPlane;
