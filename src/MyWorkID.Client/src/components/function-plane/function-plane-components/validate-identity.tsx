import { useEffect, useState } from "react";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/signal-r-service";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { CardContent, CircularProgress } from "@mui/material";
import { useToast } from "@/hooks/use-toast";
import { ValidateIdentitySvg } from "@/assets/svg/validate-identity-svg";

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

  useEffect(() => {
    getVerifiedIdConnection().then((connection) => {
      if (connection.state === HubConnectionState.Disconnected) {
        connection.on("HideQrCode", () => {
          console.log("HideQrCode received");
          setVerifiedIdDisplay({
            visible: false,
            qrCodeBase64: undefined,
            loading: false,
          });
        });
        console.log("Connecting to SignalR hub...", connection);
        connection.start();
      }
    });
  }, []);
  const { toast } = useToast();
  const validateIdentity = () => {
    setVerifiedIdDisplay({
      visible: false,
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
          toast({
            variant: "destructive",
            title: "Something went wrong during Identity validation.",
            description: "No license for verified ID is assigned",
          });
        }
      })
      .catch((error) => {
        setVerifiedIdDisplay({
          visible: false,
          qrCodeBase64: undefined,
          loading: false,
        });
        toast({
          variant: "destructive",
          title: "Something went wrong during Identity validation.",
          description: error.response.statusText,
        });
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
            <CardTitle>{ValidateIdentitySvg}</CardTitle>
          </CardHeader>
          <CardFooter className="action-card__footer">
            Validate Identity
          </CardFooter>
        </Card>
      ) : (
        <>
          {!verifiedIdDisplay.loading ? (
            <Card
              className="action-card__qr-code"
              onClick={() => {
                validateIdentity();
              }}
            >
              <CardContent>
                <div>
                  <img alt="QrCode" src={verifiedIdDisplay.qrCodeBase64}></img>
                </div>
              </CardContent>
            </Card>
          ) : (
            <div>
              <CircularProgress />
            </div>
          )}
        </>
      )}
    </div>
  );
};
