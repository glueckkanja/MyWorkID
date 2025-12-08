import { useEffect, useState } from "react";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/signal-r-service";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useToast } from "@/hooks/use-toast";
import ValidateIdentitySvg from "../../../assets/svg/validate-identity.svg";
import { Spinner } from "@/components/ui/spinner";

type VerifiedIdDisplay = {
  visible: boolean;
  qrCodeBase64?: string;
  loading: boolean;
};

export const ValidateIdentity = (/*props: ActionResultProps<any>*/) => {
  const [verifiedIdDisplay, setVerifiedIdDisplay] = useState<VerifiedIdDisplay>(
    {
      visible: false,
      qrCodeBase64: undefined,
      loading: false,
    }
  );

  const { toastError, toastException, toastSuccess } = useToast();

  const hideVerifiedIdDisplay = () => {
    setVerifiedIdDisplay({
      visible: false,
      qrCodeBase64: undefined,
      loading: false,
    });
  };

  useEffect(() => {
    getVerifiedIdConnection().then((connection) => {
      if (connection.state === HubConnectionState.Disconnected) {
        connection.on("HideQrCode", () => {
          hideVerifiedIdDisplay();
        });

        connection.on("VerificationSuccess", () => {
          hideVerifiedIdDisplay();
          toastSuccess("Identity Verified", "Your identity has been successfully verified.");
        });

        connection.on("VerificationFailed", (errorMessage: string) => {
          hideVerifiedIdDisplay();
          toastError(errorMessage || "Identity verification failed. Please try again.");
        });
        connection.start();
      }
    });
  }, [toastError, toastSuccess]);
  
  const validateIdentity = () => {
    setVerifiedIdDisplay({
      visible: true,
      qrCodeBase64: undefined,
      loading: true,
    });

    verifyIdentity()
      .then((result) => {
        if (result.data?.qrCode != null) {
          setVerifiedIdDisplay({
            visible: true,
            qrCodeBase64: result.data?.qrCode,
            loading: false,
          });
        } else {
          setVerifiedIdDisplay({
            visible: false,
            qrCodeBase64: undefined,
            loading: false,
          });
          toastError();
        }
      })
      .catch((error) => {
        setVerifiedIdDisplay({
          visible: false,
          qrCodeBase64: undefined,
          loading: false,
        });
        toastException(error);
      });
  };

  return (
    <div>
      {!verifiedIdDisplay.visible ? (
        <Card
          className="action-card"
          onClick={() => {
            validateIdentity();
          }}
        >
          <CardHeader>
            <CardTitle>
              <img src={ValidateIdentitySvg} alt="ValidateIdentity" />
            </CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Validate Identity
          </CardFooter>
        </Card>
      ) : verifiedIdDisplay.loading ? (
        <Card className="action-card__container__loading">
          <CardContent>
            <div className="action-card__loading">
              <Spinner />
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card
          className="action-card__qr-code"
          onClick={() => {
            validateIdentity();
          }}
        >
          <CardContent>
            <div>
              <img alt="QrCode" className="action-card__qr-code__img" src={verifiedIdDisplay.qrCodeBase64}></img>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};
