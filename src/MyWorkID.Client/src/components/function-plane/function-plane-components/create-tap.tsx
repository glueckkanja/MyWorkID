import { useEffect, useState } from "react";
import { TFunctionProps } from "../../../types";
import { generateTAP, revokeTemporaryAccessPass } from "../../../services/api-service";
import { Card } from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
import { Spinner } from "@/components/ui/spinner";
import { Trash2 } from "lucide-react";
import CreateTapSvgIcon from "@/assets/svg/create-tap.svg";
import CreateTapSvgIconFilled from "@/assets/svg/create-tap-filled.svg";

type TAPDisplay = {
  visible: boolean;
  value: string;
  temporaryAccessPassId: string;
  loading: boolean;
  revoking: boolean;
};

export const CreateTAP = (props: TFunctionProps) => {
  const [tapDisplay, setTapDisplay] = useState<TAPDisplay>({
    visible: false,
    value: "",
    temporaryAccessPassId: "",
    loading: false,
    revoking: false,
  });
  const { toastException, toastError, toastSuccess } = useToast();

  useEffect(() => {
    if (props.comingFromRedirect) {
      createTAP();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.comingFromRedirect]);

  const createTAP = async () => {
    setTapDisplay({
      visible: true,
      value: "",
      temporaryAccessPassId: "",
      loading: true,
      revoking: false,
    });
    generateTAP()
      .then((result) => {
        if (
          result.status === "success" &&
          !!result.data?.temporaryAccessPassword &&
          result.data?.temporaryAccessPassword.trim().length > 0
        ) {
          setTapDisplay({
            visible: true,
            value: result.data?.temporaryAccessPassword,
            temporaryAccessPassId: result.data?.temporaryAccessPassId,
            loading: false,
            revoking: false,
          });
        } else {
          setTapDisplay({
            visible: false,
            value: "",
            temporaryAccessPassId: "",
            loading: false,
            revoking: false,
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
          revoking: false,
        });
      });
  };

  const handleRevokeTemporaryAccessPass = async () => {
    setTapDisplay((previous) => ({ ...previous, revoking: true }));
    revokeTemporaryAccessPass(tapDisplay.temporaryAccessPassId)
      .then((result) => {
        if (result.status === "success") {
          setTapDisplay({
            visible: false,
            value: "",
            temporaryAccessPassId: "",
            loading: false,
            revoking: false,
          });
          toastSuccess("Temporary Access Pass revoked.", "");
        } else {
          setTapDisplay((previous) => ({ ...previous, revoking: false }));
          toastError();
        }
      })
      .catch((error) => {
        toastException(error);
        setTapDisplay((previous) => ({ ...previous, revoking: false }));
      });
  };

  const copyToClipboard = () => {
    navigator.clipboard
      .writeText(tapDisplay.value)
      .then(() => {
        toastSuccess("Copied to clipboard!", "");
      })
      .catch((error) => {
        toastError(error);
      });
  };

  const isShowingToken = tapDisplay.visible && !tapDisplay.loading;

  const handleCardClick = () => {
    if (tapDisplay.loading) {
      return;
    }
    if (isShowingToken) {
      copyToClipboard();
    } else {
      createTAP();
    }
  };

  const cardClassName = [
    "tap-action-card",
    tapDisplay.loading ? "tap-action-card--loading" : "",
    isShowingToken ? "tap-action-card--filled" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div>
      <Card className={cardClassName} onClick={handleCardClick}>
        <span className="tap-action-card__icon">
          <img
            src={isShowingToken ? CreateTapSvgIconFilled : CreateTapSvgIcon}
            alt="CreateTapIcon"
          />
        </span>
        <span className="tap-action-card__label">
          Create Temporary Access Pass
        </span>
        <span className="tap-action-card__spinner-overlay">
          <Spinner />
        </span>
        <span className="tap-action-card__token no-select">
          {tapDisplay.value}
        </span>
        <button
          className="tap-action-card__revoke"
          onClick={(event) => {
            event.stopPropagation();
            handleRevokeTemporaryAccessPass();
          }}
          disabled={tapDisplay.revoking}
          title="Revoke Temporary Access Pass"
          tabIndex={isShowingToken ? 0 : -1}
        >
          <Trash2 size={22} />
        </button>
      </Card>
    </div>
  );
};
