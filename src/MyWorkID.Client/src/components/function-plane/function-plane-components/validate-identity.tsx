import { memo, useEffect, useRef, useState } from "react";
import { Panel } from "../../panel";
import { InformationBox } from "../information-box";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState, type HubConnection } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/signal-r-service";
import {
  ValidateIdentityPanelProps,
  VerifiedIdAction,
  VerifyState,
} from "../../../types";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import { formatValidityDuration } from "@/lib/verified-id-status";

// Import Icons
import { FaceCheckCameraIcon } from "@/components/ui/icons/face-check-camera-icon";
import { AlertWarningIcon } from "@/components/ui/icons/alert-warning-icon";
import VerifiedIdentityIllustrationSvg from "../../../assets/svg/verified-identity-illustration.svg";

export const ValidateIdentityPanel = memo(function ValidateIdentityPanel({
  open,
  onClose,
  onVerified,
  verificationAction,
  verifiedIdStatus,
}: Readonly<ValidateIdentityPanelProps>) {
  const [verifyState, setVerifyState] = useState<VerifyState>({
    status: "idle",
  });
  const { toastError, toastException, toastInfo, toastSuccess } = useToast();

  const onVerifiedRef = useRef(onVerified);
  useEffect(() => {
    onVerifiedRef.current = onVerified;
  }, [onVerified]);

  const openRef = useRef(open);
  useEffect(() => {
    openRef.current = open;
  }, [open]);

  const requiresVerificationPerAction =
    verifiedIdStatus?.requiresVerificationPerAction ?? false;
  const requiresVerificationPerActionRef = useRef(
    requiresVerificationPerAction,
  );
  useEffect(() => {
    requiresVerificationPerActionRef.current = requiresVerificationPerAction;
  }, [requiresVerificationPerAction]);

  // Action the running verification was started for; undefined for the standalone verification.
  const runningVerificationActionRef = useRef<VerifiedIdAction | undefined>(
    undefined,
  );

  useEffect(() => {
    const connectToVerifiedId = async () => {
      let connection: HubConnection;
      try {
        connection = await getVerifiedIdConnection();
      } catch (connectionError) {
        console.error("Could not get Verified ID connection", connectionError);
        return;
      }

      if (connection.state !== HubConnectionState.Disconnected) {
        return;
      }

      connection.on("HideQrCode", () => {
        // A verification started from an action keeps the panel open until it completes, so the
        // action can continue right away.
        if (runningVerificationActionRef.current) {
          if (openRef.current) {
            setVerifyState({ status: "scanned" });
          }
          return;
        }
        setVerifyState({ status: "idle" });
        toastInfo("QR code scanned", "Please continue on your mobile device.");
        onClose();
      });

      connection.on("VerificationSuccess", () => {
        const verificationAction = runningVerificationActionRef.current;
        runningVerificationActionRef.current = undefined;
        setVerifyState({ status: "idle" });
        if (!verificationAction) {
          toastSuccess(
            "Identity Verified",
            "Your identity has been successfully verified.",
          );
          onClose();
        } else if (
          openRef.current ||
          !requiresVerificationPerActionRef.current
        ) {
          // An abandoned verification only counts when a validity window is configured.
          toastSuccess(
            "Identity Verified",
            openRef.current
              ? "You can continue with the action now."
              : "Your identity has been successfully verified.",
          );
        }
        // For an action, onVerified opens that action's panel, which replaces this one.
        onVerifiedRef.current?.(verificationAction);
      });

      connection.on("VerificationFailed", (errorMessage: string) => {
        runningVerificationActionRef.current = undefined;
        setVerifyState({ status: "idle" });
        toastError(
          errorMessage || "Identity verification failed. Please try again.",
        );
      });

      try {
        await connection.start();
      } catch (startError) {
        console.error("Could not start Verified ID connection", startError);
      }
    };

    void connectToVerifiedId();
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
    runningVerificationActionRef.current = verificationAction;
    verifyIdentity(verificationAction)
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
    if (!verifiedIdStatus?.required || verifiedIdStatus.loading) {
      return null;
    }
    // Every action asks for its own verification, so there is no validity to explain.
    if (verifiedIdStatus.requiresVerificationPerAction) {
      return null;
    }
    if (verifiedIdStatus.isVerified) {
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

    if (verifyState.status === "scanned") {
      return (
        <>
          <div className="panel-loading">
            <Spinner />
          </div>
          <InformationBox>QR code scanned. Complete the face check on your mobile device.
              The action continues here automatically afterwards.</InformationBox>
        </>
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
          <InformationBox>Scan this QR code with the Microsoft Authenticator app to present
              your Verified ID credential.</InformationBox>
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
        <InformationBox>You'll be asked to take a real-time selfie. The system matches it
            against your Microsoft Entra ID photo. Results are stored securely.</InformationBox>
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
});
