import { useEffect, useState } from "react";
import { Panel } from "../../panel";
import { dismissUserRisk } from "../../../services/api-service";
import { CircleCheckIcon } from "@/components/ui/icons/circle-check-icon";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import { RiskLevel } from "@/hooks/use-user-profile";

type DismissUserRiskPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
  onDismissed?: () => void;
  riskLevel: RiskLevel;
  riskLabel: string;
};

const renderConfirmationBody = (riskLevel: RiskLevel, riskLabel: string) => {
  if (riskLevel === "high" || riskLevel === "medium" || riskLevel === "low") {
    return {
      variant: "danger" as const,
      content: (
        <>
          Your account is currently flagged with <strong>{riskLabel}</strong>.
          By dismissing, you confirm this was expected behavior (e.g. travel,
          new device). All active sessions will be revoked.
        </>
      ),
    };
  }
  if (riskLevel === "none") {
    return {
      variant: "neutral" as const,
      content: (
        <>
          Your account currently has <strong>no active risk</strong>. You can
          still confirm your account is safe — all active sessions will be
          revoked.
        </>
      ),
    };
  }
  return {
    variant: "neutral" as const,
    content: (
      <>
        Your current risk state could not be retrieved. Dismissing will still
        confirm your account is safe. All active sessions will be revoked.
      </>
    ),
  };
};

export const DismissUserRiskPanel = ({
  open,
  onClose,
  comingFromRedirect,
  onDismissed,
  riskLevel,
  riskLabel,
}: DismissUserRiskPanelProps) => {
  const confirmation = renderConfirmationBody(riskLevel, riskLabel);
  const [submitting, setSubmitting] = useState(false);
  const { toastException, toastSuccess } = useToast();

  const triggerDismiss = () => {
    setSubmitting(true);
    dismissUserRisk()
      .then(() => {
        toastSuccess(
          "Risk Dismissed",
          "Your risk status has been cleared. All previous sessions have been revoked.",
        );
        onDismissed?.();
        onClose();
      })
      .catch((error) => {
        toastException(error);
      })
      .finally(() => {
        setSubmitting(false);
      });
  };

  // auto-dismiss risk when returning from authentication redirect
  useEffect(() => {
    if (open && comingFromRedirect) {
      triggerDismiss();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, comingFromRedirect]);

  // reset submitting state when panel is closed
  useEffect(() => {
    if (!open) {
      setSubmitting(false);
    }
  }, [open]);

  return (
    <Panel
      open={open}
      title="Dismiss User Risk"
      subtitle="Confirm that your account is safe and request risk removal."
      onClose={onClose}
    >
      {submitting ? (
        <div className="panel-loading">
          <Spinner />
        </div>
      ) : (
        <>
          <div
            className={
              confirmation.variant === "neutral"
                ? "confirm-box confirm-box--neutral"
                : "confirm-box"
            }
          >
            {confirmation.content}
          </div>
          <button
            type="button"
            className="panel-primary-button"
            onClick={triggerDismiss}
          >
            <CircleCheckIcon />
            Dismiss Risk
          </button>
        </>
      )}
    </Panel>
  );
};
