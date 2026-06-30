import { useCallback, useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import { generateTAP } from "../../../services/api-service";
import { Card } from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import { Trash2 } from "lucide-react";
import CreateTapSvgIcon from "@/assets/svg/create-tap.svg";
import CreateTapSvgIconFilled from "@/assets/svg/create-tap-filled.svg";
import { useRevokeTemporaryAccessPass } from "./revoke-tap";

type TAPDisplay = {
  visible: boolean;
  value: string;
  temporaryAccessPassId: string;
  loading: boolean;
};

export const CreateTAP = (props: TFunctionProps) => {
  const [tapDisplay, setTapDisplay] = useState<TAPDisplay>({
    visible: false,
    value: "",
    temporaryAccessPassId: "",
    loading: false,
  });
  const { toastException, toastError, toastSuccess } = useToast();

  const isShowingToken = tapDisplay.visible && !tapDisplay.loading;

  const {
    revoking,
    swipeProgress,
    isSwipeGestureActive,
    swipeStatus,
    handleRevokeTemporaryAccessPass,
    handleSwipePointerDown,
    handleSwipePointerMove,
    handleSwipePointerUp,
    handleSwipePointerCancel,
  } = useRevokeTemporaryAccessPass({
    temporaryAccessPassId: tapDisplay.temporaryAccessPassId,
    isShowingToken,
    onRevoked: () => {
      setTapDisplay({
        visible: false,
        value: "",
        temporaryAccessPassId: "",
        loading: false,
      });
    },
  });

  const hasContent = (value: string | null | undefined): value is string =>
    !!value && value.trim().length > 0;

  const createTAP = useCallback(async () => {
    setTapDisplay({
      visible: true,
      value: "",
      temporaryAccessPassId: "",
      loading: true,
    });
    generateTAP()
      .then((result) => {
        if (
          result.status === "success" &&
          hasContent(result.data?.temporaryAccessPassword) &&
          hasContent(result.data?.temporaryAccessPassId)
        ) {
          setTapDisplay({
            visible: true,
            value: result.data.temporaryAccessPassword,
            temporaryAccessPassId: result.data.temporaryAccessPassId,
            loading: false,
          });
        } else {
          setTapDisplay({
            visible: false,
            value: "",
            temporaryAccessPassId: "",
            loading: false,
          });
          toastError();
        }
      })
      .catch((error) => {
        toastException(error);
        setTapDisplay({
          visible: false,
          value: "",
          temporaryAccessPassId: "",
          loading: false,
        });
      });
  }, [toastError, toastException]);

  useEffect(() => {
    if (props.comingFromRedirect) {
      createTAP();
    }
  }, [props.comingFromRedirect, createTAP]);

  const copyToClipboard = () => {
    navigator.clipboard
      .writeText(tapDisplay.value)
      .then(() => {
        toastSuccess("Copied to clipboard!", "");
      })
      .catch((error) => {
        toastException(error);
      });
  };

  const handleCardClick = () => {
    if (swipeStatus.current) {
      swipeStatus.current = false;
      return;
    }
    if (tapDisplay.loading) {
      return;
    }
    if (isShowingToken) {
      copyToClipboard();
    } else {
      createTAP();
    }
  };

  // three different TAP card states: unclicked, loading, showing token
  const cardClassName = [
    "action-card__tap",
    tapDisplay.loading ? "action-card-loading__tap" : "",
    isShowingToken ? "action-card-filled__tap" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div>
      <Card
        className={cardClassName}
        onClick={handleCardClick}
        onPointerDown={handleSwipePointerDown}
        onPointerMove={handleSwipePointerMove}
        onPointerUp={handleSwipePointerUp}
        onPointerCancel={handleSwipePointerCancel}
      >
        <span className="action-card__tap_icon">
          <img
            src={isShowingToken ? CreateTapSvgIconFilled : CreateTapSvgIcon}
            alt="CreateTapIcon"
          />
        </span>
        <span className="action-card-label__tap">
          Create Temporary Access Pass
        </span>
        <span className="action-card-spinner-overlay__tap">
          <Spinner />
        </span>
        <span className="action-card-token__tap no-select">
          {tapDisplay.value}
        </span>
        <button
          type="button"
          className="action-card-revoke__tap"
          aria-label="Revoke Temporary Access Pass"
          onClick={(event) => {
            event.stopPropagation();
            handleRevokeTemporaryAccessPass();
          }}
          disabled={revoking}
          title="Revoke Temporary Access Pass"
          tabIndex={isShowingToken ? 0 : -1}
        >
          <Trash2 size={22} />
        </button>
        <div
          className="action-card-swipe-overlay__tap"
          style={{
            opacity: swipeProgress,
            transition: isSwipeGestureActive ? "none" : "opacity 0.3s ease",
          }}
          aria-hidden="true"
        >
          <Trash2 size={22} color="white" />
          <span className="action-card-swipe-overlay-label__tap">
            Revoke Temporary Access Pass
          </span>
        </div>
      </Card>
    </div>
  );
};
