import { useEffect, useId, useRef } from "react";
import { PanelProps } from "../types";
import { createPortal } from "react-dom";
import { CloseXIcon } from "@/components/ui/icons/close-x-icon";

export const Panel = ({
  open,
  title,
  subtitle,
  onClose,
  children,
}: PanelProps) => {
  const panelElementRef = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    if (!open) {
      return;
    }
    const previousBodyOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    const handleEscapeKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        event.preventDefault();
        onClose();
      }
    };

    window.addEventListener("keydown", handleEscapeKey);

    return () => {
      window.removeEventListener("keydown", handleEscapeKey);
      document.body.style.overflow = previousBodyOverflow;
    };
  }, [open, onClose]);

  if (!open) {
    return null;
  }

  return createPortal(
    <>
      <button
        className="panel-overlay"
        type="button"
        aria-label="Close panel"
        onClick={onClose}
      />
      <dialog
        className="panel"
        open
        ref={panelElementRef}
        aria-labelledby={titleId}
      >
        <div className="panel__handle" />
        <div className="panel__header">
          <div>
            <h3 id={titleId} className="panel__title">
              {title}
            </h3>
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
