import { useMemo, useEffect, useState } from "react";
import { getUserRiskState } from "../services/api-service";
import { RiskLabel, RiskLevel, SettingsProviderProps } from "../types";
import { SettingsContext } from "./settings-context";

const toMaxDismissibleRiskLevel = (raw: string | undefined): RiskLevel => {
  switch (raw?.toLowerCase()) {
    case RiskLabel.High:
      return RiskLevel.High;
    case RiskLabel.Medium:
      return RiskLevel.Medium;
    case RiskLabel.Low:
    default:
      return RiskLevel.Low;
  }
};

export const SettingsProvider = (props: SettingsProviderProps) => {
  const [maxDismissibleRiskLevel, setMaxDismissibleRiskLevel] =
    useState<RiskLevel>(RiskLevel.Low);

  useEffect(() => {
    getUserRiskState()
      .then((result) => {
        setMaxDismissibleRiskLevel(
          toMaxDismissibleRiskLevel(result?.maxDismissibleRiskLevel),
        );
      })
      .catch(() => {
        // Keep default (Low) on failure — same fallback as the previous hook
      });
  }, []);

  const settingsValue = useMemo(
    () => ({ maxDismissibleRiskLevel }),
    [maxDismissibleRiskLevel],
  );

  return (
    <SettingsContext.Provider value={settingsValue}>
      {props.children}
    </SettingsContext.Provider>
  );
};
