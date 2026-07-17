import { ReactNode, useEffect } from "react";
import { createPortal } from "react-dom";
import { CloseXIcon } from "@/components/ui/icons/close-x-icon";

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
      <button
        type="button"
        className="panel-overlay"
        aria-label="Close panel"
        onClick={onClose}
      />
      <dialog className="panel" open aria-label={title}>
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
            <CloseXIcon />
          </button>
        </div>
        <div className="panel__body">{children}</div>
      </dialog>
    </>,
    document.body,
  );
};
