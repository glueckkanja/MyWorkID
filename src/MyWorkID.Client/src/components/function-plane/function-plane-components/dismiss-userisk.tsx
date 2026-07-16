import { useEffect, useState } from "react";
import { Panel } from "../../panel";
import { dismissUserRisk } from "../../../services/api-service";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";

type DismissUserRiskPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
  onDismissed?: () => void;
};

export const DismissUserRiskPanel = ({
  open,
  onClose,
  comingFromRedirect,
  onDismissed,
}: DismissUserRiskPanelProps) => {
  const [submitting, setSubmitting] = useState(false);
  const { toastException, toastSuccess } = useToast();

  const triggerDismiss = () => {
    setSubmitting(true);
    dismissUserRisk()
      .then(() => {
        toastSuccess(
          "Risk Dismissed",
          "Your risk status has been cleared. All previous sessions have been revoked."
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

  useEffect(() => {
    if (open && comingFromRedirect) {
      triggerDismiss();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, comingFromRedirect]);

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
          <div className="confirm-box">
            Your account is flagged as <strong>at risk</strong>. By dismissing,
            you confirm this was expected behavior (e.g. travel, new device).
            All active sessions will be revoked.
          </div>
          <button
            type="button"
            className="panel-primary-button"
            onClick={triggerDismiss}
          >
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
            >
              <path d="M9 12l2 2 4-4" />
              <path d="M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10z" />
            </svg>
            Dismiss Risk
          </button>
        </>
      )}
    </Panel>
  );
};
