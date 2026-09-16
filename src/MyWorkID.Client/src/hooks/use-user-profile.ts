import { useCallback, useEffect, useRef, useState } from "react";
import {
  getUser,
  getUserImage,
  getUserRiskState,
} from "../services/api-service";
import {
  TGetRiskStateResponse,
  User,
  UserProfile,
} from "../types";
import { getRiskLabelFromLevel, getRiskLevelFromLabel, RiskLabel, RiskLevel } from "@/lib/risk-level";
import axios from "axios";

const RISK_STATE_POLL_INTERVAL_MILLISECONDS = 30000;

const toRiskLevel = (
  riskState: TGetRiskStateResponse | undefined
): { level: RiskLevel; label: string } => {
  const riskLevel = getRiskLevelFromLabel(riskState?.riskLevel);
  switch (riskLevel) {
    case RiskLevel.High:
    case RiskLevel.Medium:
    case RiskLevel.Low:
      return { level: riskLevel, label: `Risk Level: ${getRiskLabelFromLevel(riskLevel)}` };
    default:
      break;
  }
  const riskStateLabel = riskState?.riskState?.toLowerCase();
  if (
    riskLevel === RiskLevel.None ||
    riskStateLabel === RiskLabel.None.toLowerCase() ||
    riskStateLabel === "dismissed" ||
    riskStateLabel === "remediated" ||
    riskStateLabel === "confirmedsafe"
  ) {
    return { level: RiskLevel.None, label: "No Active Risk" };
  }
  return { level: RiskLevel.Unknown, label: "Risk State Unknown" };
};

export const useUserProfile = (): UserProfile => {
  const [user, setUser] = useState<User>();
  const [userImage, setUserImage] = useState<string>();
  const [riskLoading, setRiskLoading] = useState(true);
  const [riskLevel, setRiskLevel] = useState<RiskLevel>(RiskLevel.Unknown);
  const [riskLabel, setRiskLabel] = useState("No Active Risk");
  const pollingIntervalRef = useRef<ReturnType<typeof setInterval> | undefined>(
    undefined,
  );

  const refreshRiskState = useCallback(() => {
    getUserRiskState()
      .then((result) => {
        const { level, label } = toRiskLevel(result);
        setRiskLevel(level);
        setRiskLabel(label);
        setRiskLoading(false);
      })
      .catch((error) => {
        if (axios.isAxiosError(error) && error.response?.status === 404) {
          setRiskLevel(RiskLevel.None);
          setRiskLabel("No Active Risk");
        } else {
          console.error("Could not get risk state", error);
          setRiskLevel(RiskLevel.Unknown);
          setRiskLabel("Risk State Unknown");
        }
        setRiskLoading(false);
      });
  }, []);

  useEffect(() => {
    getUser()
      .then((profile) => {
        setUser(profile);
      })
      .catch((error) => {
        if (import.meta.env.DEV) {
          console.debug("User profile request failed", error);
        }
      });

    getUserImage()
      .then((imageBlob) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          if (typeof reader.result === "string") {
            setUserImage(reader.result);
          }
        };
        reader.readAsDataURL(imageBlob);
      })
      .catch((error) => {
        if (import.meta.env.DEV) {
          console.debug("User profile image request failed", error);
        }
      });

    refreshRiskState();
  }, [refreshRiskState]);

  useEffect(() => {
    pollingIntervalRef.current = setInterval(
      refreshRiskState,
      RISK_STATE_POLL_INTERVAL_MILLISECONDS
    );
    return () => {
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
      }
    };
  }, [refreshRiskState]);

  return {
    user,
    userImage,
    riskLoading,
    riskLevel,
    riskLabel,
    refreshRiskState,
  };
};
