import * as React from "react";
import { CheckIcon, CopyIcon } from "@radix-ui/react-icons";
import { useToast } from "@/hooks/use-toast";
import {
  Toast,
  ToastClose,
  ToastDescription,
  ToastProvider,
  ToastTitle,
  ToastViewport,
} from "@/components/ui/toast";

const COPIED_INFO_TOAST_DISMISSAL_TIME_MILLISECONDS = 2000;
const TOAST_DISPLAY_TIME_MILLISECONDS = 8000;

const CorrelationIdRow = ({
  correlationId,
}: Readonly<{ correlationId: string }>) => {
  const [copied, setCopied] = React.useState(false);
  const copiedTimerRef = React.useRef<number | undefined>(undefined);

  React.useEffect(() => {
    return () => {
      if (copiedTimerRef.current !== undefined) {
        window.clearTimeout(copiedTimerRef.current);
      }
    };
  }, []);

  const handleCopy = () => {
    navigator.clipboard
      .writeText(correlationId)
      .then(() => {
        setCopied(true);
        if (copiedTimerRef.current !== undefined) {
          window.clearTimeout(copiedTimerRef.current);
        }
        copiedTimerRef.current = window.setTimeout(() => {
          setCopied(false);
          copiedTimerRef.current = undefined;
        }, COPIED_INFO_TOAST_DISMISSAL_TIME_MILLISECONDS);
      })
      .catch((error) => {
        console.debug("Failed to copy correlation ID to clipboard", error);
      });
  };

  return (
    <ToastDescription className="text-xs opacity-70 mt-1 flex items-center gap-1">
      Correlation ID: {correlationId}
      <button
        type="button"
        onClick={handleCopy}
        className="ml-1 opacity-70 transition-opacity hover:opacity-100"
        aria-label="Copy correlation ID"
      >
        {copied ? (
          <CheckIcon className="h-5 w-5" />
        ) : (
          <CopyIcon className="h-5 w-5" />
        )}
      </button>
    </ToastDescription>
  );
};

export const Toaster = () => {
  const { toasts } = useToast();

  return (
    <ToastProvider duration={TOAST_DISPLAY_TIME_MILLISECONDS}>
      {toasts.map(
        ({ id, title, description, action, correlationId, ...props }) => (
          <Toast key={id} {...props}>
            <div className="grid gap-1">
              {title && <ToastTitle>{title}</ToastTitle>}
              {description && (
                <ToastDescription>{description}</ToastDescription>
              )}
              {correlationId && (
                <CorrelationIdRow correlationId={correlationId} />
              )}
            </div>
            {action}
            <ToastClose />
          </Toast>
        ),
      )}
      <ToastViewport />
    </ToastProvider>
  );
};
