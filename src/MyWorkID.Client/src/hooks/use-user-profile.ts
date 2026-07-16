import { useCallback, useEffect, useRef, useState } from "react";
import {
  getUser,
  getUserImage,
  getUserRiskState,
} from "../services/api-service";
import { TGetRiskStateResponse, User } from "../types";

const RISK_STATE_POLL_INTERVAL_MILLISECONDS = 30000;

export type RiskLevel = "high" | "medium" | "low" | "none" | "unknown";

export type UserProfile = {
  user: User | undefined;
  userImage: string | undefined;
  riskLoading: boolean;
  riskLevel: RiskLevel;
  riskLabel: string;
  refreshRiskState: () => void;
};

const toRiskLevel = (
  riskState: TGetRiskStateResponse | undefined
): { level: RiskLevel; label: string } => {
  const rawLevel = riskState?.riskLevel?.toLowerCase();
  if (rawLevel === "high") {
    return { level: "high", label: "Risk Level: High" };
  }
  if (rawLevel === "medium") {
    return { level: "medium", label: "Risk Level: Medium" };
  }
  if (rawLevel === "low") {
    return { level: "low", label: "Risk Level: Low" };
  }
  const rawState = riskState?.riskState?.toLowerCase();
  if (
    rawLevel === "none" ||
    rawState === "none" ||
    rawState === "dismissed" ||
    rawState === "remediated" ||
    rawState === "confirmedsafe"
  ) {
    return { level: "none", label: "No Active Risk" };
  }
  return { level: "unknown", label: "Risk State Unknown" };
};

export const useUserProfile = (): UserProfile => {
  const [user, setUser] = useState<User>();
  const [userImage, setUserImage] = useState<string>();
  const [riskLoading, setRiskLoading] = useState(true);
  const [riskLevel, setRiskLevel] = useState<RiskLevel>("unknown");
  const [riskLabel, setRiskLabel] = useState("No Active Risk");
  const pollingIntervalRef = useRef<ReturnType<typeof setInterval>>();

  const refreshRiskState = useCallback(() => {
    getUserRiskState()
      .then((result) => {
        const { level, label } = toRiskLevel(result);
        setRiskLevel(level);
        setRiskLabel(label);
        setRiskLoading(false);
      })
      .catch((error) => {
        console.error("Could not get risk state", error);
        setRiskLevel("unknown");
        setRiskLabel("Risk State Unknown");
        setRiskLoading(false);
      });
  }, []);

  useEffect(() => {
    getUser()
      .then((profile) => {
        setUser(profile);
      })
      .catch(() => {
        // ignore — user card gracefully degrades
      });

    getUserImage()
      .then((imageBlob) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          const base64 = reader.result?.toString();
          setUserImage(base64);
        };
        reader.readAsDataURL(imageBlob);
      })
      .catch(() => {
        // ignore — no profile image
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
