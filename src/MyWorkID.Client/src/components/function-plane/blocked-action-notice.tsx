import { AlertErrorIcon } from "@/components/ui/icons/alert-error-icon";
import { Spinner } from "@/components/ui/spinner";
import type { ReactNode } from "react";
import type { BlockedActionNoticeProps } from "../../types";
import { InformationBox } from "./information-box";

/**
 * Shown inside an action panel instead of its form while the action is blocked
 * (e.g. because a recent identity verification is required).
 */
export const BlockedActionNotice = ({
  reason,
  actionLabel,
  actionIcon,
  actionVariant,
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
      <BlockedActionButton
        label={actionLabel}
        icon={actionIcon}
        variant={actionVariant}
        onClick={onVerifyIdentity}
      />
    </>
  );
};

/**
 * Looks like the button that runs the action. While the action is blocked, clicking it starts the
 * identity verification first (the face check opens right away); without a way to verify it is disabled.
 */
export const BlockedActionButton = ({
  label,
  icon,
  variant = "primary",
  onClick,
}: Readonly<{
  label: string;
  icon?: ReactNode;
  variant?: "primary" | "secondary";
  onClick?: () => void;
}>) => (
  <button
    type="button"
    className={
      variant === "secondary"
        ? "panel-secondary-button"
        : "panel-primary-button"
    }
    onClick={onClick}
    disabled={!onClick}
    aria-disabled={!onClick || undefined}
  >
    {icon}
    {label}
  </button>
);
