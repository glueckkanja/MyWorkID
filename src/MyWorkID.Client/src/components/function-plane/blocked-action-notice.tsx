import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";
import { FaceCheckCameraIcon } from "@/components/ui/icons/face-check-camera-icon";
import { Spinner } from "@/components/ui/spinner";
import type { BlockedActionNoticeProps } from "../../types";
import { InformationBox } from "./information-box";

/**
 * Shown inside an action panel instead of its form while the action is blocked
 * (e.g. because a recent identity verification is required).
 */
export const BlockedActionNotice = ({
  reason,
  actionLabel,
  hint,
  onVerifyIdentity,
  showReason = true,
  waiting = false,
}: Readonly<BlockedActionNoticeProps>) => {
  if (waiting) {
    return (
      <div className="panel-loading">
        <Spinner />
      </div>
    );
  }

  return (
    <>
      {showReason && (
        <div className="confirm-box">
          <AlertErrorIcon aria-hidden="true" />
          <span>{reason}</span>
        </div>
      )}
      {hint && <InformationBox>{hint}</InformationBox>}
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
};

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
