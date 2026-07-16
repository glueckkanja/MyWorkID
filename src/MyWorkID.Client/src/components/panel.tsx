import { ReactNode, useEffect } from "react";
import { createPortal } from "react-dom";

type PanelProps = {
  open: boolean;
  title: string;
  subtitle?: string;
  onClose: () => void;
  children: ReactNode;
};

export const Panel = ({
  open,
  title,
  subtitle,
  onClose,
  children,
}: PanelProps) => {
  useEffect(() => {
    if (!open) {
      return;
    }
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        onClose();
      }
    };
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  if (!open) {
    return null;
  }

  return createPortal(
    <>
      <div
        className="panel-overlay"
        onClick={onClose}
        role="presentation"
        aria-hidden="true"
      />
      <div className="panel" role="dialog" aria-modal="true" aria-label={title}>
        <div className="panel__handle" />
        <div className="panel__header">
          <div>
            <h3 className="panel__title">{title}</h3>
            {subtitle && <p className="panel__subtitle">{subtitle}</p>}
          </div>
          <button
            type="button"
            className="panel__close"
            onClick={onClose}
            aria-label="Close"
          >
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              strokeLinecap="round"
            >
              <path d="M18 6L6 18M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div className="panel__body">{children}</div>
      </div>
    </>,
    document.body
  );
};
