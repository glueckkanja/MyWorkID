import { useEffect, useState } from "react";
import { Panel } from "../../panel";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/signal-r-service";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";

type ValidateIdentityPanelProps = {
  open: boolean;
  onClose: () => void;
};

type VerifyState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "ready"; qrCodeBase64: string };

export const ValidateIdentityPanel = ({
  open,
  onClose,
}: ValidateIdentityPanelProps) => {
  const [verifyState, setVerifyState] = useState<VerifyState>({
    status: "idle",
  });
  const { toastError, toastException, toastSuccess } = useToast();

  useEffect(() => {
    getVerifiedIdConnection().then((connection) => {
      if (connection.state === HubConnectionState.Disconnected) {
        connection.on("HideQrCode", () => {
          setVerifyState({ status: "idle" });
        });

        connection.on("VerificationSuccess", () => {
          setVerifyState({ status: "idle" });
          toastSuccess(
            "Identity Verified",
            "Your identity has been successfully verified."
          );
          onClose();
        });

        connection.on("VerificationFailed", (errorMessage: string) => {
          setVerifyState({ status: "idle" });
          toastError(
            errorMessage || "Identity verification failed. Please try again."
          );
        });

        connection.start();
      }
    });
  }, [toastError, toastSuccess, onClose]);

  useEffect(() => {
    if (!open) {
      setVerifyState({ status: "idle" });
    }
  }, [open]);

  const startVerification = () => {
    setVerifyState({ status: "loading" });
    verifyIdentity()
      .then((result) => {
        const qrCode = result.data?.qrCode;
        if (qrCode) {
          setVerifyState({ status: "ready", qrCodeBase64: qrCode });
        } else {
          setVerifyState({ status: "idle" });
          toastError();
        }
      })
      .catch((error) => {
        setVerifyState({ status: "idle" });
        toastException(error);
      });
  };

  const renderBody = () => {
    if (verifyState.status === "loading") {
      return (
        <div className="panel-loading">
          <Spinner />
        </div>
      );
    }

    if (verifyState.status === "ready") {
      return (
        <>
          <div className="verify-illustration">
            <img
              src={verifyState.qrCodeBase64}
              alt="Verified ID QR Code"
              className="verify-illustration__qr"
            />
          </div>
          <div className="form-hint">
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
            >
              <circle cx="12" cy="12" r="10" />
              <path d="M12 16v-4M12 8h.01" />
            </svg>
            <span>
              Scan this QR code with the Microsoft Authenticator app to present
              your Verified ID credential.
            </span>
          </div>
        </>
      );
    }

    return (
      <>
        <div className="verify-illustration">
          <svg
            width="80"
            height="80"
            viewBox="0 0 80 80"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
            className="verify-illustration__icon"
          >
            <rect x="8" y="8" width="64" height="64" rx="16" />
            <circle cx="40" cy="34" r="10" />
            <path d="M24 62c0-8.837 7.163-16 16-16s16 7.163 16 16" />
            <path d="M58 16l6-6M16 16l-6-6M58 64l6 6M16 64l-6 6" />
          </svg>
        </div>
        <div className="form-hint">
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
          >
            <circle cx="12" cy="12" r="10" />
            <path d="M12 16v-4M12 8h.01" />
          </svg>
          <span>
            You'll be asked to take a real-time selfie. The system matches it
            against your Microsoft Entra ID photo. Results are stored securely.
          </span>
        </div>
        <button
          type="button"
          className="panel-primary-button"
          onClick={startVerification}
        >
          <svg
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
          >
            <path d="M23 19a2 2 0 01-2 2H3a2 2 0 01-2-2V8a2 2 0 012-2h4l2-3h6l2 3h4a2 2 0 012 2z" />
            <circle cx="12" cy="13" r="4" />
          </svg>
          Start Face Check
        </button>
      </>
    );
  };

  return (
    <Panel
      open={open}
      title="Validate Identity"
      subtitle="Prove your identity using a face scan via Verified ID."
      onClose={onClose}
    >
      {renderBody()}
    </Panel>
  );
};
