import { useEffect, useState } from "react";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/signal-r-service";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { CardContent, CircularProgress } from "@mui/material";
import { useToast } from "@/hooks/use-toast";
type VerifiedIdDisplay = {
  visible: boolean;
  qrCodeBase64?: string;
  loading: boolean;
};

const svgIcon = (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="46"
    height="46"
    viewBox="0 0 46 46"
    fill="none"
  >
    <path
      d="M1.33331 12.6376V6.98543C1.33331 5.48638 1.92881 4.04872 2.9888 2.98874C4.04879 1.92875 5.48644 1.33325 6.98549 1.33325H12.6377"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="square"
    />
    <path
      d="M44.6665 12.6376V6.98543C44.6665 5.48638 44.071 4.04872 43.011 2.98874C41.9511 1.92875 40.5134 1.33325 39.0144 1.33325H33.3622"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="square"
    />
    <path
      d="M1.33331 33.3624V39.0146C1.33331 40.5137 1.92881 41.9513 2.9888 43.0113C4.04879 44.0713 5.48644 44.6668 6.98549 44.6668H12.6377"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="square"
    />
    <path
      d="M44.6665 33.3624V39.0146C44.6665 40.5137 44.071 41.9513 43.011 43.0113C41.9511 44.0713 40.5134 44.6668 39.0144 44.6668H33.3622"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="square"
    />
    <path
      d="M27.5651 34.6573V30.9456H28.6836C29.1028 30.9862 29.5258 30.9401 29.9262 30.8101C30.3267 30.6802 30.6959 30.4693 31.0107 30.1905C31.3256 29.9118 31.5793 29.5712 31.7561 29.1902C31.9328 28.8092 32.0287 28.3959 32.0378 27.9763V25.0069H35.1835C35.2715 25.0074 35.3582 24.9869 35.4367 24.9473C35.5151 24.9077 35.583 24.85 35.6347 24.7791C35.6863 24.7081 35.7203 24.626 35.7338 24.5393C35.7473 24.4527 35.7399 24.3641 35.7123 24.2809C33.6166 17.8062 32.1004 10.123 25.179 8.86099C23.6685 8.569 22.1138 8.59123 20.6123 8.92626C19.1108 9.2613 17.6949 9.90192 16.4534 10.808C15.2119 11.714 14.1716 12.866 13.3977 14.1916C12.6237 15.5172 12.1329 16.9879 11.9559 18.5114C11.7777 20.4457 12.0891 22.3937 12.8617 24.1769C13.6342 25.9602 14.8432 27.5217 16.378 28.7186V34.6573"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="square"
    />
  </svg>
);
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
  const { toastError, toastException } = useToast();
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
            <CardTitle>{svgIcon}</CardTitle>
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
