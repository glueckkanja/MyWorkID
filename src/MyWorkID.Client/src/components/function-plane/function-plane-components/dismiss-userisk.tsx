import { memo, useCallback, useEffect, useMemo, useState } from "react";
import { Panel } from "../../panel";
import { dismissUserRisk } from "../../../services/api-service";
import { DismissUserRiskPanelProps } from "../../../types";
import { CircleCheckIcon } from "@/components/ui/icons/circle-check-icon";
import { AlertWarningIcon } from "@/components/ui/icons/alert-warning-icon";
import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";
import { InformationCircleIcon } from "@/components/ui/icons/information-circle-icon";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import { RiskLevel } from "@/lib/risk-level";

const renderConfirmationBody = (riskLevel: RiskLevel, riskLabel: string) => {
  if (
    riskLevel === RiskLevel.High ||
    riskLevel === RiskLevel.Medium ||
    riskLevel === RiskLevel.Low
  ) {
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
  if (riskLevel === RiskLevel.None) {
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

export const DismissUserRiskPanel = memo(function DismissUserRiskPanel({
  open,
  onClose,
  comingFromRedirect,
  onDismissed,
  riskLevel,
  riskLabel,
  canDismiss,
  dismissDisabledReason,
}: DismissUserRiskPanelProps) {
  const confirmation = useMemo(
    () => renderConfirmationBody(riskLevel, riskLabel),
    [riskLevel, riskLabel],
  );
  const [submitting, setSubmitting] = useState(false);
  const { toastException, toastSuccess } = useToast();

  const handleClose = useCallback(() => {
    setSubmitting(false);
    onClose();
  }, [onClose]);

  const triggerDismiss = useCallback(() => {
    setSubmitting(true);
    dismissUserRisk()
      .then(() => {
        toastSuccess(
          "Risk Dismissed",
          "Your risk status has been cleared. All previous sessions have been revoked.",
        );
        onDismissed?.();
        handleClose();
      })
      .catch((error) => {
        setSubmitting(false);
        toastException(error);
      });
  }, [toastSuccess, toastException, onDismissed, handleClose]);

  // auto-dismiss risk when returning from authentication redirect
  useEffect(() => {
    if (!open || !comingFromRedirect || !canDismiss) {
      return;
    }
    const timeoutId = setTimeout(triggerDismiss, 0);
    return () => clearTimeout(timeoutId);
  }, [open, comingFromRedirect, canDismiss, triggerDismiss]);

  return (
    <Panel
      open={open}
      title="Dismiss User Risk"
      subtitle="Confirm that your account is safe and request risk removal."
      onClose={handleClose}
    >
      {submitting ? (
        <div className="panel-loading">
          <Spinner />
        </div>
      ) : (
        <>
          {!canDismiss && dismissDisabledReason ? (
            <div className="confirm-box">
              <AlertErrorIcon aria-hidden="true" />
              <span>{dismissDisabledReason}</span>
            </div>
          ) : confirmation.variant === "neutral" ? (
            <div className="confirm-box confirm-box--neutral">
              <InformationCircleIcon aria-hidden="true" />
              <span>{confirmation.content}</span>
            </div>
          ) : (
            <div className="confirm-box confirm-box--warning">
              <AlertWarningIcon aria-hidden="true" />
              <span>{confirmation.content}</span>
            </div>
          )}
          <button
            type="button"
            className="panel-primary-button"
            onClick={triggerDismiss}
            disabled={!canDismiss}
            aria-disabled={!canDismiss || undefined}
          >
            <CircleCheckIcon />
            Dismiss Risk
          </button>
        </>
      )}
    </Panel>
  );
});
