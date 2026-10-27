import { useEffect, useEffectEvent, useState } from "react";
import { Panel } from "../../panel";
import { CreateTapPanelProps, TapState } from "../../../types";
import {
  generateTAP,
  revokeTemporaryAccessPass,
} from "../../../services/api-service";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";

// Import Icons
import { CircleXIcon } from "@/components/ui/icons/circle-x-icon";
import { CheckmarkIcon } from "@/components/ui/icons/checkmark-icon";
import { CopyDocumentIcon } from "@/components/ui/icons/copy-document-icon";
import { RefreshArrowIcon } from "@/components/ui/icons/refresh-arrow-icon";
import { InformationCircleIcon } from "@/components/ui/icons/information-circle-icon";
import { BlockedActionNotice } from "./blocked-action-notice";

export const CreateTapPanel = ({
  open,
  onClose,
  comingFromRedirect,
  blockedReason,
}: Readonly<CreateTapPanelProps>) => {
  const [tap, setTap] = useState<TapState>({ status: "empty" });
  const [copiedTap, setCopiedTap] = useState(false);
  const { toastError, toastException } = useToast();

  // Expects the caller to have set the "loading" state already.
  const requestTemporaryAccessPass = () => {
    generateTAP()
      .then((result) => {
        const password = result.data?.temporaryAccessPassword;
        const id = result.data?.temporaryAccessPassId;
        if (
          result.status === "success" &&
          !!password &&
          password.trim().length > 0 &&
          !!id
        ) {
          setTap({ status: "ready", id, password });
        } else {
          setTap({ status: "empty" });
          toastError();
        }
      })
      .catch((error) => {
        setTap({ status: "empty" });
        toastException(error);
      });
  };

  const createTap = () => {
    setTap({ status: "loading" });
    requestTemporaryAccessPass();
  };

  const revokeTap = () => {
    if (tap.status !== "ready") {
      return;
    }
    const idToRevoke = tap.id;
    setTap({ status: "revoking" });
    revokeTemporaryAccessPass(idToRevoke)
      .then((result) => {
        if (result.status === "success") {
          setTap({ status: "empty" });
        } else {
          toastError();
          setTap({ status: "ready", id: idToRevoke, password: tap.password });
        }
      })
      .catch((error) => {
        toastException(error);
        setTap({ status: "ready", id: idToRevoke, password: tap.password });
      });
  };

  const copyTap = () => {
    if (tap.status !== "ready") {
      return;
    }
    navigator.clipboard
      .writeText(tap.password)
      .then(() => {
        setCopiedTap(true);
        window.setTimeout(() => setCopiedTap(false), 2000);
      })
      .catch((error) => {
        toastException(error);
      });
  };

  // auto start TAP creation if coming from redirect: the loading state is set while rendering,
  // the request itself is sent from the effect below
  const redirectCreationRequested = open && comingFromRedirect;
  const [previousRedirectCreationRequested, setPreviousRedirectCreationRequested] =
    useState(false);
  if (redirectCreationRequested !== previousRedirectCreationRequested) {
    setPreviousRedirectCreationRequested(redirectCreationRequested);
    if (redirectCreationRequested && !blockedReason) {
      setTap({ status: "loading" });
    }
  }

  const requestTemporaryAccessPassFromRedirect = useEffectEvent(() => {
    if (!blockedReason) {
      requestTemporaryAccessPass();
    }
  });
  useEffect(() => {
    if (redirectCreationRequested) {
      requestTemporaryAccessPassFromRedirect();
    }
  }, [redirectCreationRequested]);

  // clear TAP state when panel is closed
  const [previousOpen, setPreviousOpen] = useState(open);
  if (open !== previousOpen) {
    setPreviousOpen(open);
    if (!open) {
      setTap({ status: "empty" });
      setCopiedTap(false);
    }
  }

  const renderBody = () => {
    if (blockedReason) {
      return (
        <BlockedActionNotice
          reason={blockedReason}
          actionLabel="Generate Access Pass"
        />
      );
    }

    if (tap.status === "loading" || tap.status === "revoking") {
      return (
        <div className="panel-loading">
          <Spinner />
        </div>
      );
    }

    if (tap.status === "empty") {
      return (
        <>
          <div className="form-hint">
            <InformationCircleIcon />
            <span>
              Generate a one-time code to sign in on a new device without your
              regular credentials.
            </span>
          </div>
          <button
            type="button"
            className="panel-primary-button"
            onClick={createTap}
          >
            Generate Access Pass
          </button>
        </>
      );
    }

    return (
      <>
        <div className="tap-result">
          <div className="tap-result__label">Your Access Pass</div>
          <div className="tap-result__code">{tap.password}</div>
          <button
            type="button"
            className={
              copiedTap
                ? "tap-copy-button tap-copy-button--copied"
                : "tap-copy-button"
            }
            onClick={copyTap}
          >
            {copiedTap ? (
              <>
                <CheckmarkIcon strokeWidth={2.5} />
                Copied!
              </>
            ) : (
              <>
                <CopyDocumentIcon />
                Copy Code
              </>
            )}
          </button>
        </div>
        <div className="form-hint">
          <InformationCircleIcon />
          <span>
            Use this code to sign in on your new device. Once expired, you can
            generate a new pass.
          </span>
        </div>
        <button
          type="button"
          className="panel-secondary-button"
          onClick={createTap}
        >
          <RefreshArrowIcon />
          Create New Pass
        </button>
        <button
          type="button"
          className="panel-secondary-button panel-secondary-button--danger"
          onClick={revokeTap}
        >
          <CircleXIcon />
          Revoke Access Pass
        </button>
      </>
    );
  };

  return (
    <Panel
      open={open}
      title="Temporary Access Pass"
      subtitle="A one-time code to sign in without your regular credentials."
      onClose={onClose}
    >
      {renderBody()}
    </Panel>
  );
};
