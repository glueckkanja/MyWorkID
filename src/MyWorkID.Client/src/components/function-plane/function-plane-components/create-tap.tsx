import { memo, useEffect, useEffectEvent, useState } from "react";
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
import { BlockedActionNotice } from "../blocked-action-notice";

const PANEL_DESCRIPTION =
  "Generate a one-time code to sign in on a new device without your regular credentials.";
const PANEL_DESCRIPTION_WITH_PASS =
  "Use this code to sign in on your new device. Once expired, you can generate a new pass.";

export const CreateTapPanel = memo(function CreateTapPanel({
  open,
  onClose,
  comingFromRedirect,
  blockedReason,
  showBlockedReason,
  waitingForVerification,
  onVerifyIdentity,
  onCompleted,
}: Readonly<CreateTapPanelProps>) {
  const [tap, setTap] = useState<TapState>({ status: "empty" });
  const [copiedTap, setCopiedTap] = useState(false);
  // Set while the identity verification started from this panel temporarily closes it, so an
  // existing pass survives the round trip and can still be used or revoked afterwards.
  const [leavingForVerification, setLeavingForVerification] = useState(false);
  const { toastError, toastException } = useToast();

  // Expects the caller to have set the "loading" state already. A failed request keeps the pass that
  // was shown before, because it has not been replaced.
  const requestTemporaryAccessPass = (
    previousTemporaryAccessPass: TapState,
    startedRightAfterVerification = false,
  ) => {
    const restorePreviousTemporaryAccessPass = () => {
      setTap(
        previousTemporaryAccessPass.status === "ready"
          ? previousTemporaryAccessPass
          : { status: "empty" },
      );
    };
    generateTAP(startedRightAfterVerification)
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
          onCompleted?.();
        } else {
          restorePreviousTemporaryAccessPass();
          toastError();
        }
      })
      .catch((error) => {
        restorePreviousTemporaryAccessPass();
        toastException(error);
      });
  };

  const createTap = () => {
    setTap({ status: "loading" });
    requestTemporaryAccessPass(tap);
  };

  const verifyIdentity = onVerifyIdentity
    ? () => {
        setLeavingForVerification(true);
        onVerifyIdentity();
      }
    : undefined;

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
  // the request itself is sent from the effect below. The redirect is consumed once its request
  // is dispatched, so a later toggle of comingFromRedirect (e.g. after a failed verification
  // status poll recovers) cannot generate another TAP. Until then the request waits while blocked.
  const [redirectConsumed, setRedirectConsumed] = useState(false);
  const [passReplacedByRedirect, setPassReplacedByRedirect] =
    useState<TapState>({ status: "empty" });
  if (open && comingFromRedirect && !blockedReason && !redirectConsumed) {
    setRedirectConsumed(true);
    setPassReplacedByRedirect(tap);
    setTap({ status: "loading" });
  }

  const requestTemporaryAccessPassFromRedirect = useEffectEvent(() => {
    requestTemporaryAccessPass(passReplacedByRedirect, true);
  });
  useEffect(() => {
    if (redirectConsumed) {
      requestTemporaryAccessPassFromRedirect();
    }
  }, [redirectConsumed]);

  // clear TAP state when panel is closed, unless it only closed for the identity verification
  // started from it: cancelling or failing that verification must not lose the still active pass.
  const [previousOpen, setPreviousOpen] = useState(open);
  if (open !== previousOpen) {
    setPreviousOpen(open);
    setLeavingForVerification(false);
    if (!open) {
      if (!leavingForVerification) {
        setTap({ status: "empty" });
      }
      setCopiedTap(false);
      // The next verification or redirect may start a pass again.
      setRedirectConsumed(false);
    }
  }

  const renderBody = () => {
    // An existing pass stays usable (and revocable) even when generating a new one is blocked,
    // so only replace the body while there is no pass to revoke.
    const hasExistingTemporaryAccessPass =
      tap.status === "ready" || tap.status === "revoking";
    if (blockedReason && !hasExistingTemporaryAccessPass) {
      return (
        <BlockedActionNotice
          reason={blockedReason}
          actionLabel="Generate Access Pass"
          onVerifyIdentity={verifyIdentity}
          showReason={showBlockedReason}
          waiting={waitingForVerification}
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
        <button
          type="button"
          className="panel-primary-button"
          onClick={createTap}
        >
          Generate Access Pass
        </button>
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
        {blockedReason ? (
          <BlockedActionNotice
            reason={blockedReason}
            actionLabel="Create New Pass"
            actionIcon={<RefreshArrowIcon />}
            actionVariant="secondary"
            onVerifyIdentity={verifyIdentity}
            showReason={showBlockedReason}
            waiting={waitingForVerification}
          />
        ) : (
          <button
            type="button"
            className="panel-secondary-button"
            onClick={createTap}
          >
            <RefreshArrowIcon />
            Create New Pass
          </button>
        )}
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
      subtitle={
        tap.status === "ready" || tap.status === "revoking"
          ? PANEL_DESCRIPTION_WITH_PASS
          : PANEL_DESCRIPTION
      }
      onClose={onClose}
    >
      {renderBody()}
    </Panel>
  );
});
