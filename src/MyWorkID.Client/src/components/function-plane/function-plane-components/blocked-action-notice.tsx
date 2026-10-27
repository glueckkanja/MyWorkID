import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";

type BlockedActionNoticeProps = {
  /** Explains why the action is currently unavailable. */
  reason: string;
  /** Label of the disabled primary button that would normally trigger the action. */
  actionLabel: string;
};

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
