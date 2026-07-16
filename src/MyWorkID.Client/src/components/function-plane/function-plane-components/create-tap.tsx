import { useEffect, useState } from "react";
import { Panel } from "../../panel";
import {
  generateTAP,
  revokeTemporaryAccessPass,
} from "../../../services/api-service";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";

type CreateTapPanelProps = {
  open: boolean;
  onClose: () => void;
  comingFromRedirect: boolean;
};

type TapState =
  | { status: "empty" }
  | { status: "loading" }
  | { status: "revoking" }
  | { status: "ready"; id: string; password: string };

export const CreateTapPanel = ({
  open,
  onClose,
  comingFromRedirect,
}: CreateTapPanelProps) => {
  const [tap, setTap] = useState<TapState>({ status: "empty" });
  const [justCopied, setJustCopied] = useState(false);
  const { toastError, toastException } = useToast();

  const createTap = () => {
    setTap({ status: "loading" });
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

  const revoke = () => {
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

  const copyCode = () => {
    if (tap.status !== "ready") {
      return;
    }
    navigator.clipboard
      .writeText(tap.password)
      .then(() => {
        setJustCopied(true);
        window.setTimeout(() => setJustCopied(false), 2000);
      })
      .catch((error) => {
        toastException(error);
      });
  };

  useEffect(() => {
    if (open && comingFromRedirect) {
      createTap();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, comingFromRedirect]);

  useEffect(() => {
    if (!open) {
      setTap({ status: "empty" });
      setJustCopied(false);
    }
  }, [open]);

  const renderBody = () => {
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
              justCopied
                ? "tap-copy-button tap-copy-button--copied"
                : "tap-copy-button"
            }
            onClick={copyCode}
          >
            {justCopied ? (
              <>
                <svg
                  width="14"
                  height="14"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2.5"
                  strokeLinecap="round"
                >
                  <path d="M20 6L9 17l-5-5" />
                </svg>
                Copied!
              </>
            ) : (
              <>
                <svg
                  width="14"
                  height="14"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                >
                  <rect x="9" y="9" width="13" height="13" rx="2" />
                  <path d="M5 15H4a2 2 0 01-2-2V4a2 2 0 012-2h9a2 2 0 012 2v1" />
                </svg>
                Copy Code
              </>
            )}
          </button>
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
            Use this code to sign in on your new device. Once expired, you can
            generate a new pass.
          </span>
        </div>
        <button
          type="button"
          className="panel-secondary-button"
          onClick={createTap}
        >
          <svg
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
          >
            <path d="M23 4v6h-6" />
            <path d="M20.49 15A9 9 0 116.36 5.64L23 10" />
          </svg>
          Create New Pass
        </button>
        <button
          type="button"
          className="panel-secondary-button panel-secondary-button--danger"
          onClick={revoke}
        >
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
            <path d="M15 9l-6 6M9 9l6 6" />
          </svg>
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
