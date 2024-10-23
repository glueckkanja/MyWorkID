import { useTheme } from "@mui/material";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Skeleton } from "@/components/ui/skeleton";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  getUser,
  getUserImage,
  getUserRiskState,
} from "../../services/ApiService";
import { TGetRiskStateResponse, User } from "../../types";

type RiskUserState = {
  loading: boolean;
  data?: TGetRiskStateResponse;
  displayValue?: string;
};

const RISK_STATE_UPDATE_POLLING_INTERVAL_IN_MILLISECONDS = 30000; // 30 seconds

export const UserDisplay = () => {
  const theme = useTheme();
  const [user, setUser] = useState<User>();
  const [userImage, setUserImage] = useState<string>();
  const [riskUserState, setRiskUserState] = useState<RiskUserState>({
    loading: true,
    data: undefined,
  });

  const riskStatePollingIntervalRef = useRef<NodeJS.Timeout>();

  const updateRiskState = useCallback(() => {
    getUserRiskState()
      .then((result) => {
        setRiskUserState({
          loading: false,
          data: result,
          displayValue: result?.riskLevel ?? result?.riskState ?? "UNKNOWN",
        });
      })
      .catch((e) => {
        console.error("Could not get risk state", e);
        setRiskUserState({
          loading: false,
          data: undefined,
          displayValue: "UNKNOWN",
        });
      });
  }, [setRiskUserState]);

  useEffect(() => {
    getUser().then((usr) => {
      setUser(usr);
    });
    getUserImage()
      .then((imgBlob) => {
        const reader = new FileReader();
        reader.readAsDataURL(imgBlob);
        reader.onloadend = () => {
          const base64String = reader.result?.toString();
          setUserImage(base64String);
        };
      })
      .catch(() => {
        // Ignore - this is thrown if no image is set
      });

    updateRiskState();
  }, [updateRiskState]);

  useEffect(() => {
    riskStatePollingIntervalRef.current = setInterval(
      updateRiskState,
      RISK_STATE_UPDATE_POLLING_INTERVAL_IN_MILLISECONDS
    );
    return () => {
      if (riskStatePollingIntervalRef.current) {
        clearInterval(riskStatePollingIntervalRef.current);
      }
    };
  }, [updateRiskState]);

  const getRiskStateColor = useCallback(
    (value?: string) => {
      switch (value?.toLocaleLowerCase()) {
        case "none":
        case "low":
          return theme.palette.success.main;
        case "medium":
          return theme.palette.warning.main;
        case "high":
          return theme.palette.error.main;
        case "unknown":
          return theme.palette.grey[500];
        case undefined:
        case null:
        default:
          return theme.palette.info.main;
      }
    },
    [theme]
  );

  return (
    <div style={{display:"flex", justifyContent:"center"}}>
      <div style={{ display: "flex", flexDirection:"column", paddingTop: "41px", alignItems:"center" }}>
        <Avatar style={{ width: 100, height: 100, borderRadius: "32px" }}>
          <AvatarImage src={userImage} />
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <div>
          <div style={{textAlign: "center",}}>
            <span
              style={{
                fontFamily: "Lato",
                fontSize: "24px",
                fontStyle: "normal",
                fontWeight: "700",
                color: "#000520",
              }}
            >
              {user?.displayName}
            </span>
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <div
                style={{
                  textAlign: "right",
                  fontFamily: "Lato",
                  fontSize: "16px",
                  fontStyle: "normal",
                  fontWeight: "400",
                  color: "#7A7D8A",
                }}
              >
                Risk State:
              </div>
              <div
                style={{ color: getRiskStateColor(riskUserState.displayValue) }}
              >
                {riskUserState.loading && (
                  <>
                    <Skeleton className="h-[125px] w-[250px] rounded-xl" />
                    <div className="space-y-2">
                      <Skeleton className="h-4 w-[250px]" />
                      <Skeleton className="h-4 w-[200px]" />
                    </div>
                  </>
                )}
                {riskUserState.displayValue}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
