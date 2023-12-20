import { Avatar, Skeleton, useTheme } from "@mui/material";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  getUser,
  getUserImage,
  getUserRiskState,
} from "../../services/ApiService";
import { setInterval, clearInterval } from "timers";
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
    <div className="user_display">
      <Avatar sx={{ width: 100, height: 100 }} src={userImage}></Avatar>
      <div className="user_display__user_info__container">
        <div>
          <h3 className="no_margin">{user?.displayName}</h3>
          <div style={{ display: "flex", gap: "0.5rem" }}>
            <div>Risk State:</div>
            <div
              style={{ color: getRiskStateColor(riskUserState.displayValue) }}
            >
              {riskUserState.loading && (
                <Skeleton
                  variant="text"
                  width={100}
                  sx={{ fontSize: "1rem" }}
                />
              )}
              {riskUserState.displayValue}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
