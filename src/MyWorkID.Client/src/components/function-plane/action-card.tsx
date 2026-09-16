import { ActionCardProps } from "../../types";
import { RightChevronIcon } from "../ui/icons/right-chevron-icon";

export const ActionCard = ({
  icon,
  title,
  description,
  highlighted,
  onClick,
  disabled,
  disabledReason,
}: ActionCardProps) => {
  const classes = ["action-card"];
  if (highlighted) {
    classes.push("action-card--highlighted");
  }
  if (disabled) {
    classes.push("action-card--disabled");
  }
  return (
    <button
      type="button"
      className={classes.join(" ")}
      onClick={onClick}
      disabled={disabled}
      aria-disabled={disabled || undefined}
      title={disabled ? disabledReason : undefined}
    >
      <span className="action-card__icon" aria-hidden="true">
        {icon}
      </span>
      <span className="action-card__content">
        <span className="action-card__title">{title}</span>
        <span className="action-card__desc">
          {disabled && disabledReason ? disabledReason : description}
        </span>
      </span>
      <span className="action-card__arrow" aria-hidden="true">
        <RightChevronIcon />
      </span>
    </button>
  );
};
