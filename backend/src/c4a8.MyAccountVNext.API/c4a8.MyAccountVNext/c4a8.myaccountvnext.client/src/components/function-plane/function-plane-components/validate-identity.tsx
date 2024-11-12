import { useEffect, useState } from "react";
import { verifyIdentity } from "../../../services/api-service";
import { HubConnectionState } from "@microsoft/signalr";
import { getVerifiedIdConnection } from "../../../services/SignalRService";
import { Card, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { CardContent, CircularProgress } from "@mui/material";
type VerifiedIdDisplay = {
  visible: boolean;
  qrCodeBase64?: string;
  loading: boolean;
};

const ERROR_IMAGE_BASE64 =
  "data:image/png;base64,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";

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
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M44.6665 12.6376V6.98543C44.6665 5.48638 44.071 4.04872 43.011 2.98874C41.9511 1.92875 40.5134 1.33325 39.0144 1.33325H33.3622"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M1.33331 33.3624V39.0146C1.33331 40.5137 1.92881 41.9513 2.9888 43.0113C4.04879 44.0713 5.48644 44.6668 6.98549 44.6668H12.6377"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M44.6665 33.3624V39.0146C44.6665 40.5137 44.071 41.9513 43.011 43.0113C41.9511 44.0713 40.5134 44.6668 39.0144 44.6668H33.3622"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
    />
    <path
      d="M27.5651 34.6573V30.9456H28.6836C29.1028 30.9862 29.5258 30.9401 29.9262 30.8101C30.3267 30.6802 30.6959 30.4693 31.0107 30.1905C31.3256 29.9118 31.5793 29.5712 31.7561 29.1902C31.9328 28.8092 32.0287 28.3959 32.0378 27.9763V25.0069H35.1835C35.2715 25.0074 35.3582 24.9869 35.4367 24.9473C35.5151 24.9077 35.583 24.85 35.6347 24.7791C35.6863 24.7081 35.7203 24.626 35.7338 24.5393C35.7473 24.4527 35.7399 24.3641 35.7123 24.2809C33.6166 17.8062 32.1004 10.123 25.179 8.86099C23.6685 8.569 22.1138 8.59123 20.6123 8.92626C19.1108 9.2613 17.6949 9.90192 16.4534 10.808C15.2119 11.714 14.1716 12.866 13.3977 14.1916C12.6237 15.5172 12.1329 16.9879 11.9559 18.5114C11.7777 20.4457 12.0891 22.3937 12.8617 24.1769C13.6342 25.9602 14.8432 27.5217 16.378 28.7186V34.6573"
      stroke="white"
      stroke-width="2"
      stroke-linecap="square"
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

  const validateIdentity = () => {
    setVerifiedIdDisplay({
      visible: false,
      qrCodeBase64: undefined,
      loading: true,
    });

    verifyIdentity()
      .then((result) => {
        setVerifiedIdDisplay({
          visible: true,
          qrCodeBase64: result.data?.qrCode || ERROR_IMAGE_BASE64,
          loading: false,
        });
      })
      .catch((error) => {
        console.error("Something went wrong during TAP generation.", error);
        setVerifiedIdDisplay({
          visible: true,
          qrCodeBase64: ERROR_IMAGE_BASE64,
          loading: false,
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
                  <img src={verifiedIdDisplay.qrCodeBase64}></img>
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
