import { useCallback, useEffect, useState } from "react";

// Import services
import {
  getPendingAction,
  handleRedirectPromise,
} from "../../services/msal-service";
import { useSignedInUser } from "../../contexts/signed-in-user-provider";
import { Role } from "../../services/roles-service";
import { EApiFunctionTypes, PanelKey } from "../../types";
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
const VERIFICATION_STATUS_LOADING_REASON =
  "Loading your account status. This action becomes available once loading has finished.";
const VERIFICATION_STATUS_ERROR_REASON =
  "Your identity verification status could not be loaded. Please try again later.";
const VERIFICATION_NOT_POSSIBLE_REASON =
  "Identity verification is required for this action, but you don't have permission to validate your identity. Please contact your administrator.";

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

  const openPasswordPanel = useCallback(() => setActivePanel("password"), []);
  const openTemporaryAccessPassPanel = useCallback(
    () => setActivePanel("tap"),
    [],
  );
  const openDismissPanel = useCallback(() => setActivePanel("dismiss"), []);
  const openValidatePanel = useCallback(() => setActivePanel("validate"), []);

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

  // When the optional "require recent Verified ID" feature is enabled, sensitive actions are
  // blocked until the user completed a face check within the configured validity window.
  // Until the status is known, actions stay blocked so that neither a click nor an automatic
  // resume after an authentication redirect can run against an unknown requirement.
  const verificationStatusLoading = verifiedIdStatus.loading;
  const verificationStatusFailed =
    !verifiedIdStatus.loading && verifiedIdStatus.error;
  const verificationStatusUnknown =
    verificationStatusLoading || verificationStatusFailed;
  const verificationBlocked =
    !verificationStatusUnknown &&
    verifiedIdStatus.required &&
    !verifiedIdStatus.isVerified;
  const verificationHasExpired =
    verificationBlocked && verifiedIdStatus.lastVerifiedAt !== undefined;
  const actionsBlocked = verificationStatusUnknown || verificationBlocked;
  let verificationBlockedReason: string | undefined;
  if (verificationStatusLoading) {
    verificationBlockedReason = VERIFICATION_STATUS_LOADING_REASON;
  } else if (verificationStatusFailed) {
    verificationBlockedReason = VERIFICATION_STATUS_ERROR_REASON;
  } else if (verificationBlocked) {
    if (!canValidateIdentity) {
      verificationBlockedReason = VERIFICATION_NOT_POSSIBLE_REASON;
    } else if (verificationHasExpired) {
      verificationBlockedReason = VERIFICATION_EXPIRED_REASON;
    } else {
      verificationBlockedReason = VERIFICATION_REQUIRED_REASON;
    }
  }

  const dismissAllowedForCurrentLevel =
    profile.riskLoading ||
    canDismissRiskLevel(profile.riskLevel, maxDismissibleRiskLevel);
  const dismissDisabledReason = dismissAllowedForCurrentLevel
    ? verificationBlockedReason
    : `Your ${getRiskLabelFromLevel(profile.riskLevel)} risk level cannot be dismissed via self-service. Please contact your administrator.`;

  const showRecommendedDismiss =
    canDismissRisk &&
    !actionsBlocked &&
    !profile.riskLoading &&
    dismissAllowedForCurrentLevel &&
    (profile.riskLevel === RiskLevel.High ||
      profile.riskLevel === RiskLevel.Medium);

  const showRecommendedValidate = verificationBlocked && canValidateIdentity;
  const showRecommendedSection = showRecommendedDismiss || showRecommendedValidate;

  const closePanel = useCallback(() => {
    setActivePanel(null);
    setRedirectAction(undefined);
  }, []);

  return (
    <main className="app-main">
      <UserDisplay profile={profile} verifiedIdStatus={verifiedIdStatus} />

      {showRecommendedSection && (
        <>
          <div className="section-label">Recommended</div>
          <div className="actions-list">
            {showRecommendedValidate && (
              <ActionCard
                icon={IDENTITY_VERIFICATION_ICON}
                title="Validate Identity"
                description={
                  verificationHasExpired
                    ? "Your verification has expired. Verify again to unlock all actions"
                    : "Verify who you are to unlock all actions"
                }
                highlighted
                onClick={openValidatePanel}
              />
            )}
            {showRecommendedDismiss && (
              <ActionCard
                icon={DISMISS_ICON}
                title="Dismiss Risk"
                description="Resolve your risk status to regain full access"
                highlighted
                onClick={openDismissPanel}
              />
            )}
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
            description="Clear your account risk status"
            onClick={openDismissPanel}
          />
        )}
        {canValidateIdentity && !showRecommendedValidate && (
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
            !actionsBlocked &&
            redirectAction === EApiFunctionTypes.PASSWORD_RESET
          }
          blockedReason={verificationBlockedReason}
        />
      )}
      {canCreateTap && (
        <CreateTapPanel
          open={activePanel === "tap" || activePanel === "revokeTap"}
          onClose={closePanel}
          comingFromRedirect={
            !actionsBlocked &&
            redirectAction === EApiFunctionTypes.CREATE_TAP
          }
          blockedReason={verificationBlockedReason}
        />
      )}
      {canDismissRisk && (
        <DismissUserRiskPanel
          open={activePanel === "dismiss"}
          onClose={closePanel}
          comingFromRedirect={
            !actionsBlocked &&
            redirectAction === EApiFunctionTypes.DISMISS_USER_RISK
          }
          onDismissed={profile.refreshRiskState}
          riskLevel={profile.riskLevel}
          riskLabel={profile.riskLabel}
          canDismiss={dismissAllowedForCurrentLevel && !actionsBlocked}
          dismissDisabledReason={dismissDisabledReason}
        />
      )}
      {canValidateIdentity && (
        <ValidateIdentityPanel
          open={activePanel === "validate"}
          onClose={closePanel}
          onVerified={verifiedIdStatus.refreshAfterVerification}
          verifiedIdStatus={verifiedIdStatus}
        />
      )}
    </main>
  );
};

export default FunctionPlane;
