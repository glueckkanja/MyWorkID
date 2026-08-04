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

function CorrelationIdRow({
  correlationId,
}: Readonly<{ correlationId: string }>) {
  const [copied, setCopied] = React.useState(false);

  function handleCopy() {
    navigator.clipboard.writeText(correlationId);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

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
}

export function Toaster() {
  const { toasts } = useToast();

  return (
    <ToastProvider duration={8000}>
      {toasts.map(function ({
        id,
        title,
        description,
        action,
        correlationId,
        ...props
      }) {
        return (
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
        );
      })}
      <ToastViewport />
    </ToastProvider>
  );
}
