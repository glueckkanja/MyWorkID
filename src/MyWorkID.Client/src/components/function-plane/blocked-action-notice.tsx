import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";
import type { BlockedActionNoticeProps } from "../../types";

/**
 * Shown inside an action panel instead of its form while the action is blocked
 * (e.g. because a recent identity verification is required).
 */
export const BlockedActionNotice = ({
  reason,
  actionLabel,
}: Readonly<BlockedActionNoticeProps>) => (
  <>
    <div className="confirm-box">
      <AlertErrorIcon aria-hidden="true" />
      <span>{reason}</span>
    </div>
    <button
      type="button"
      className="panel-primary-button"
      disabled
      aria-disabled
    >
      {actionLabel}
    </button>
  </>
);
