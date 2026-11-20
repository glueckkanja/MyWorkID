import { memo, useEffect, useRef, useState } from "react";
import { Panel } from "../../panel";
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

const PANEL_DESCRIPTIONS: Record<VerifyState["status"], string> = {
  idle: "Prove your identity using a face scan via Verified ID. You'll be asked to take a real-time selfie, which is matched against your Microsoft Entra ID photo. Results are stored securely.",
  loading:
    "Prove your identity using a face scan via Verified ID. You'll be asked to take a real-time selfie, which is matched against your Microsoft Entra ID photo. Results are stored securely.",
  ready:
    "Scan this QR code with the Microsoft Authenticator app to present your Verified ID credential.",
  scanned:
    "QR code scanned. Complete the face check on your mobile device. The action continues here automatically afterwards.",
};

const getPanelDescription = (status: VerifyState["status"]) =>
  PANEL_DESCRIPTIONS[status];

// A presentation request started from this panel. Hub events are sent to every connection of the
// user, so they are matched to it by the request ID.
type StartedPresentation = {
  requestId?: string;
  verificationAction?: VerifiedIdAction;
  // Set when the panel was closed or another verification was started before this one completed.
  abandoned: boolean;
};

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

  const requiresVerificationPerAction =
    verifiedIdStatus?.requiresVerificationPerAction ?? false;
  const requiresVerificationPerActionRef = useRef(
    requiresVerificationPerAction,
  );
  useEffect(() => {
    requiresVerificationPerActionRef.current = requiresVerificationPerAction;
  }, [requiresVerificationPerAction]);

  // The presentation started for the current visit of this panel.
  const currentPresentationRef = useRef<StartedPresentation | undefined>(
    undefined,
  );
  // Every presentation started from this panel whose outcome is still outstanding, by request ID.
  // Events for request IDs missing here belong to another tab and are ignored.
  const startedPresentationsRef = useRef(
    new Map<string, StartedPresentation>(),
  );

  useEffect(() => {
    openRef.current = open;
    if (!open && currentPresentationRef.current) {
      currentPresentationRef.current.abandoned = true;
      currentPresentationRef.current = undefined;
    }
  }, [open]);

  useEffect(() => {
    const findStartedPresentation = (requestId: string | null | undefined) =>
      requestId ? startedPresentationsRef.current.get(requestId) : undefined;

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

      connection.on("HideQrCode", (requestId: string | null) => {
        const presentation = findStartedPresentation(requestId);
        if (!presentation || presentation !== currentPresentationRef.current) {
          return;
        }
        // A verification started from an action keeps the panel open until it completes, so the
        // action can continue right away.
        if (presentation.verificationAction) {
          if (openRef.current) {
            setVerifyState({ status: "scanned" });
          }
          return;
        }
        setVerifyState({ status: "idle" });
        toastInfo("QR code scanned", "Please continue on your mobile device.");
        onClose();
      });

      connection.on("VerificationSuccess", (requestId: string | null) => {
        const presentation = findStartedPresentation(requestId);
        if (!presentation) {
          return;
        }
        startedPresentationsRef.current.delete(presentation.requestId!);
        const { verificationAction, abandoned } = presentation;

        if (abandoned) {
          // The verification is recorded anyway. It only counts when a validity window is configured,
          // and it never resumes the action, so onVerified just cleans up.
          if (!verificationAction || !requiresVerificationPerActionRef.current) {
            toastSuccess(
              "Identity Verified",
              "Your identity has been successfully verified.",
            );
          }
          onVerifiedRef.current?.(verificationAction, true);
          return;
        }

        currentPresentationRef.current = undefined;
        setVerifyState({ status: "idle" });
        if (verificationAction) {
          toastSuccess(
            "Identity Verified",
            "You can continue with the action now.",
          );
        } else {
          toastSuccess(
            "Identity Verified",
            "Your identity has been successfully verified.",
          );
          onClose();
        }
        // For an action, onVerified opens that action's panel, which replaces this one.
        onVerifiedRef.current?.(verificationAction, false);
      });

      connection.on(
        "VerificationFailed",
        (requestId: string | null, errorMessage: string) => {
          const presentation = findStartedPresentation(requestId);
          if (!presentation) {
            return;
          }
          startedPresentationsRef.current.delete(presentation.requestId!);
          if (presentation.abandoned) {
            return;
          }
          currentPresentationRef.current = undefined;
          setVerifyState({ status: "idle" });
          toastError(
            errorMessage || "Identity verification failed. Please try again.",
          );
        },
      );

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
    if (currentPresentationRef.current) {
      currentPresentationRef.current.abandoned = true;
    }
    const presentation: StartedPresentation = {
      verificationAction,
      abandoned: false,
    };
    currentPresentationRef.current = presentation;
    setVerifyState({ status: "loading" });
    verifyIdentity(verificationAction)
      .then((result) => {
        const requestId = result.data?.requestId;
        const qrCode = result.data?.qrCode;
        if (requestId) {
          // Registered even when abandoned meanwhile, so its completion can still be cleaned up.
          presentation.requestId = requestId;
          startedPresentationsRef.current.set(requestId, presentation);
        }
        if (presentation.abandoned) {
          return;
        }
        if (requestId && qrCode) {
          setVerifyState({ status: "ready", qrCodeBase64: qrCode });
        } else {
          currentPresentationRef.current = undefined;
          setVerifyState({ status: "idle" });
          toastError();
        }
      })
      .catch((error) => {
        if (presentation.abandoned) {
          return;
        }
        currentPresentationRef.current = undefined;
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
      subtitle={getPanelDescription(verifyState.status)}
      onClose={onClose}
    >
      {renderBody()}
    </Panel>
  );
});
