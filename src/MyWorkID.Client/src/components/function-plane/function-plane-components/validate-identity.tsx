import { useEffect, useRef, useState } from "react";
import { Panel } from "../../panel";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/signal-r-service";
import { ValidateIdentityPanelProps, VerifyState } from "../../../types";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import { formatValidityDuration } from "@/lib/verified-id-status";

// Import Icons
import { FaceCheckCameraIcon } from "@/components/ui/icons/face-check-camera-icon";
import { InformationCircleIcon } from "@/components/ui/icons/information-circle-icon";
import { AlertWarningIcon } from "@/components/ui/icons/alert-warning-icon";
import VerifiedIdentityIllustrationSvg from "../../../assets/svg/verified-identity-illustration.svg";

export const ValidateIdentityPanel = ({
  open,
  onClose,
  onVerified,
  verifiedIdStatus,
}: Readonly<ValidateIdentityPanelProps>) => {
  const [verifyState, setVerifyState] = useState<VerifyState>({
    status: "idle",
  });
  const { toastError, toastException, toastInfo, toastSuccess } = useToast();

  const onVerifiedRef = useRef(onVerified);
  useEffect(() => {
    onVerifiedRef.current = onVerified;
  }, [onVerified]);

  useEffect(() => {
    getVerifiedIdConnection().then((connection) => {
      if (connection.state === HubConnectionState.Disconnected) {
        connection.on("HideQrCode", () => {
          setVerifyState({ status: "idle" });
          toastInfo(
            "QR code scanned",
            "Please continue on your mobile device.",
          );
          onClose();
        });

        connection.on("VerificationSuccess", () => {
          setVerifyState({ status: "idle" });
          toastSuccess(
            "Identity Verified",
            "Your identity has been successfully verified.",
          );
          onVerifiedRef.current?.();
          onClose();
        });

        connection.on("VerificationFailed", (errorMessage: string) => {
          setVerifyState({ status: "idle" });
          toastError(
            errorMessage || "Identity verification failed. Please try again.",
          );
        });

        connection.start();
      }
    });
  }, [toastError, toastInfo, toastSuccess, onClose]);

  const [previousOpen, setPreviousOpen] = useState(open);
  if (open !== previousOpen) {
    setPreviousOpen(open);
    if (!open) {
      setVerifyState({ status: "idle" });
    }
  }

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

  const renderRequirementHint = () => {
    if (
      !verifiedIdStatus?.required ||
      verifiedIdStatus.loading ||
      verifiedIdStatus.isVerified
    ) {
      return null;
    }
    const validity = formatValidityDuration(verifiedIdStatus.validityInMinutes);
    return (
      <div className="confirm-box confirm-box--warning">
        <AlertWarningIcon aria-hidden="true" />
        <span>
          {verifiedIdStatus.lastVerifiedAt ? (
            <>
              Your identity verification has <strong>expired</strong>.
            </>
          ) : (
            <>
              Your identity is <strong>not verified</strong> yet.
            </>
          )}{" "}
          Resetting your password, creating an access pass and dismissing your
          risk require a verification that is not older than {validity}.
        </span>
      </div>
    );
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
            <InformationCircleIcon />
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
          <img
            src={VerifiedIdentityIllustrationSvg}
            alt=""
            aria-hidden="true"
            className="verify-illustration__icon"
          />
        </div>
        {renderRequirementHint()}
        <div className="form-hint">
          <InformationCircleIcon />
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
          <FaceCheckCameraIcon />
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
