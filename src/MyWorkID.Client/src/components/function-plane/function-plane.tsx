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

// Hoisted so the ActionCard `icon` prop is a stable reference across renders,
// which lets React.memo(ActionCard) bail out on unrelated FunctionPlane re-renders.
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

  const openDismissPanel = useCallback(() => setActivePanel("dismiss"), []);

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

  const dismissAllowedForCurrentLevel =
    profile.riskLoading ||
    canDismissRiskLevel(profile.riskLevel, maxDismissibleRiskLevel);
  const dismissDisabledReason = !dismissAllowedForCurrentLevel
    ? `Your ${getRiskLabelFromLevel(profile.riskLevel)} risk level cannot be dismissed via self-service. Please contact your administrator.`
    : undefined;

  const showRecommendedDismiss =
    canDismissRisk &&
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
              description="Resolve your risk status to regain full access"
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
            onClick={() => setActivePanel("password")}
          />
        )}
        {canCreateTap && (
          <ActionCard
            icon={TEMPORARY_ACCESS_PASS_ICON}
            title="Temporary Access Pass"
            description="Get a one-time code to sign in or set up a new device"
            onClick={() => setActivePanel("tap")}
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
        {canValidateIdentity && (
          <ActionCard
            icon={IDENTITY_VERIFICATION_ICON}
            title="Validate Identity"
            description="Verify who you are using Face Check"
            onClick={() => setActivePanel("validate")}
          />
        )}
      </div>

      {canResetPassword && (
        <PasswordResetPanel
          open={activePanel === "password"}
          onClose={closePanel}
          comingFromRedirect={
            redirectAction === EApiFunctionTypes.PASSWORD_RESET
          }
        />
      )}
      {canCreateTap && (
        <CreateTapPanel
          open={activePanel === "tap" || activePanel === "revokeTap"}
          onClose={closePanel}
          comingFromRedirect={redirectAction === EApiFunctionTypes.CREATE_TAP}
        />
      )}
      {canDismissRisk && (
        <DismissUserRiskPanel
          open={activePanel === "dismiss"}
          onClose={closePanel}
          comingFromRedirect={
            redirectAction === EApiFunctionTypes.DISMISS_USER_RISK
          }
          onDismissed={profile.refreshRiskState}
          riskLevel={profile.riskLevel}
          riskLabel={profile.riskLabel}
          canDismiss={dismissAllowedForCurrentLevel}
          dismissDisabledReason={dismissDisabledReason}
        />
      )}
      {canValidateIdentity && (
        <ValidateIdentityPanel
          open={activePanel === "validate"}
          onClose={closePanel}
        />
      )}
    </main>
  );
};

export default FunctionPlane;
