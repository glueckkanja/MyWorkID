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

type PanelKey = "password" | "tap" | "dismiss" | "validate" | null;

const PANEL_BY_ACTION: Record<EApiFunctionTypes, Exclude<PanelKey, null> | null> =
  {
    [EApiFunctionTypes.PASSWORD_RESET]: "password",
    [EApiFunctionTypes.CREATE_TAP]: "tap",
    [EApiFunctionTypes.DISMISS_USER_RISK]: "dismiss",
    [EApiFunctionTypes.VALIDATE_IDENTITY]: "validate",
    [EApiFunctionTypes.UNKNOWN]: null,
  };

const PasswordIcon = () => (
  <svg
    width="20"
    height="20"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="1.8"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
    <path d="M7 11V7a5 5 0 0110 0v4" />
    <circle cx="12" cy="16" r="1" />
  </svg>
);

const TapIcon = () => (
  <svg
    width="20"
    height="20"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="1.8"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <rect x="2" y="6" width="20" height="12" rx="2" />
    <path d="M12 12h.01" />
    <path d="M17 12h.01" />
    <path d="M7 12h.01" />
  </svg>
);

const IdentityIcon = () => (
  <svg
    width="20"
    height="20"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="1.8"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" />
    <circle cx="12" cy="7" r="4" />
    <path d="M16 3.13a4 4 0 010 7.75" />
  </svg>
);

const DismissIcon = () => (
  <svg
    width="20"
    height="20"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="1.8"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <path d="M9 12l2 2 4-4" />
    <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z" />
  </svg>
);

const FunctionPlane = () => {
  const [activePanel, setActivePanel] = useState<PanelKey>(null);
  const [redirectAction, setRedirectAction] = useState<EApiFunctionTypes>();
  const signedInUserInfo = useSignedInUser();
  const profile = useUserProfile();

  useEffect(() => {
    handleRedirectPromise().then((authenticationResult) => {
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
      } catch {
        // ignore — no valid pending action
      }
    });
  }, []);

  const hasRole = (role: string) =>
    !!signedInUserInfo?.roles?.includes(role);

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
            icon={<PasswordIcon />}
            title="Reset Password"
            description="Set a new password for your account"
            onClick={() => setActivePanel("password")}
          />
        )}
        {canCreateTap && (
          <ActionCard
            icon={<TapIcon />}
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
            icon={<IdentityIcon />}
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
          open={activePanel === "tap"}
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
