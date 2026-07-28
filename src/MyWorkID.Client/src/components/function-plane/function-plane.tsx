import { useEffect, useState } from "react";
import {
  getPendingAction,
  handleRedirectPromise,
} from "../../services/msal-service";
import { EApiFunctionTypes } from "../../types";
import { UserDisplay } from "./user-display";
import { useSignedInUser } from "../../contexts/signed-in-user-provider";
import { Role } from "../../services/roles-service";
import { useUserProfile } from "@/hooks/use-user-profile";
import { ActionCard } from "./action-card";
import { PasswordResetPanel } from "./function-plane-components/password-reset";
import { CreateTapPanel } from "./function-plane-components/create-tap";
import { DismissUserRiskPanel } from "./function-plane-components/dismiss-userisk";
import { ValidateIdentityPanel } from "./function-plane-components/validate-identity";

// Import Icons
import { CircleCheckIcon } from "@/components/ui/icons/circle-check-icon";
import { IdentityVerificationIcon } from "@/components/ui/icons/identity-verification-icon";
import { PasswordLockIcon } from "@/components/ui/icons/password-lock-icon";
import { TemporaryAccessPassIcon } from "@/components/ui/icons/temporary-access-pass-icon";

type PanelKey =
  | "password"
  | "tap"
  | "revokeTap"
  | "dismiss"
  | "validate"
  | null;

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

const DismissIcon = () => (
  <CircleCheckIcon width={20} height={20} strokeWidth={1.8} />
);

const FunctionPlane = () => {
  const [activePanel, setActivePanel] = useState<PanelKey>(null);
  const [redirectAction, setRedirectAction] = useState<EApiFunctionTypes>();
  const signedInUserInfo = useSignedInUser();
  const profile = useUserProfile();

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

  const showRecommendedDismiss =
    canDismissRisk &&
    !profile.riskLoading &&
    (profile.riskLevel === "high" || profile.riskLevel === "medium");

  const closePanel = () => setActivePanel(null);

  return (
    <main className="app-main">
      <UserDisplay profile={profile} />

      {showRecommendedDismiss && (
        <>
          <div className="section-label">Recommended</div>
          <div className="actions-list">
            <ActionCard
              icon={<DismissIcon />}
              title="Dismiss Risk"
              description="Resolve your risk status to regain full access"
              highlighted
              onClick={() => setActivePanel("dismiss")}
            />
          </div>
        </>
      )}

      <div className="section-label">All Actions</div>
      <div className="actions-list">
        {canResetPassword && (
          <ActionCard
            icon={<PasswordLockIcon />}
            title="Reset Password"
            description="Set a new password for your account"
            onClick={() => setActivePanel("password")}
          />
        )}
        {canCreateTap && (
          <ActionCard
            icon={<TemporaryAccessPassIcon />}
            title="Temporary Access Pass"
            description="Get a one-time code to sign in or set up a new device"
            onClick={() => setActivePanel("tap")}
          />
        )}
        {canDismissRisk && !showRecommendedDismiss && (
          <ActionCard
            icon={<DismissIcon />}
            title="Dismiss User Risk"
            description="Clear your account risk status"
            onClick={() => setActivePanel("dismiss")}
          />
        )}
        {canValidateIdentity && (
          <ActionCard
            icon={<IdentityVerificationIcon />}
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
