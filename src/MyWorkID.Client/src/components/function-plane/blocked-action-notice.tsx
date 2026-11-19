import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";
import { FaceCheckCameraIcon } from "@/components/ui/icons/face-check-camera-icon";
import type { BlockedActionNoticeProps } from "../../types";

/**
 * Shown inside an action panel instead of its form while the action is blocked
 * (e.g. because a recent identity verification is required).
 */
export const BlockedActionNotice = ({
  reason,
  actionLabel,
  onVerifyIdentity,
  showReason = true,
}: Readonly<BlockedActionNoticeProps>) => (
  <>
    {showReason && (
      <div className="confirm-box">
        <AlertErrorIcon aria-hidden="true" />
        <span>{reason}</span>
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
