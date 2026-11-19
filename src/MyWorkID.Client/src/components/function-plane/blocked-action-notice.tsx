import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";
import { AlertWarningIcon } from "@/components/ui/icons/alert-warning-icon";
import { FaceCheckCameraIcon } from "@/components/ui/icons/face-check-camera-icon";
import { InformationCircleIcon } from "@/components/ui/icons/information-circle-icon";
import type { BlockedActionNoticeProps } from "../../types";

/**
 * Shown inside an action panel instead of its form while the action is blocked
 * (e.g. because a recent identity verification is required).
 */
export const BlockedActionNotice = ({
  reason,
  reasonVariant = "error",
  actionLabel,
  hint,
  onVerifyIdentity,
  showReason = true,
}: Readonly<BlockedActionNoticeProps>) => (
  <>
    {showReason && (
      <div
        className={
          reasonVariant === "warning"
            ? "confirm-box confirm-box--warning"
            : "confirm-box"
        }
      >
        {reasonVariant === "warning" ? (
          <AlertWarningIcon aria-hidden="true" />
        ) : (
          <AlertErrorIcon aria-hidden="true" />
        )}
        <span>{reason}</span>
      </div>
    )}
    {hint && (
      <div className="form-hint">
        <InformationCircleIcon />
        <span>{hint}</span>
      </div>
    )}
    {onVerifyIdentity ? (
      <VerifyIdentityButton onClick={onVerifyIdentity} />
    ) : (
      <button
        type="button"
        className="panel-primary-button"
        disabled
        aria-disabled
      >
        {actionLabel}
      </button>
    )}
  </>
);

/**
 * Primary panel button that starts the identity verification for a blocked action.
 */
export const VerifyIdentityButton = ({
  onClick,
}: Readonly<{ onClick: () => void }>) => (
  <button type="button" className="panel-primary-button" onClick={onClick}>
    <FaceCheckCameraIcon />
    Verify Identity
  </button>
);
