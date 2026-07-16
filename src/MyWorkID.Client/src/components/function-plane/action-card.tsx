import { ReactNode } from "react";

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
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
        >
          <path d="M9 18l6-6-6-6" />
        </svg>
      </span>
    </button>
  );
};
