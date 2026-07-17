import { ReactNode } from "react";
import { RightChevronIcon } from "../ui/icons/right-chevron-icon";

type ActionCardProps = {
  icon: ReactNode;
  title: string;
  description: string;
  highlighted?: boolean;
  onClick: () => void;
};

export const ActionCard = ({
  icon,
  title,
  description,
  highlighted,
  onClick,
}: ActionCardProps) => {
  return (
    <button
      type="button"
      className={
        highlighted ? "action-card action-card--highlighted" : "action-card"
      }
      onClick={onClick}
    >
      <span className="action-card__icon" aria-hidden="true">
        {icon}
      </span>
      <span className="action-card__content">
        <span className="action-card__title">{title}</span>
        <span className="action-card__desc">{description}</span>
      </span>
      <span className="action-card__arrow" aria-hidden="true">
        <RightChevronIcon />
      </span>
    </button>
  );
};
