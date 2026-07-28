import { ReactNode, useEffect, useId, useRef } from "react";
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
  const dialogRef = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    if (!open) {
      return;
    }
    const dialog = dialogRef.current;
    if (!dialog) {
      return;
    }
    if (!dialog.open) {
      dialog.showModal();
    }
    const handleBackdropClick = (event: MouseEvent) => {
      if (event.target === dialog) {
        onClose();
      }
    };
    const handleCancel = (event: Event) => {
      event.preventDefault();
      onClose();
    };
    dialog.addEventListener("click", handleBackdropClick);
    dialog.addEventListener("cancel", handleCancel);
    return () => {
      dialog.removeEventListener("click", handleBackdropClick);
      dialog.removeEventListener("cancel", handleCancel);
    };
  }, [open, onClose]);

  if (!open) {
    return null;
  }

  return createPortal(
    <dialog ref={dialogRef} className="panel" aria-labelledby={titleId}>
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
    </dialog>,
    document.body,
  );
};
