import { useMemo, useEffect, useState } from "react";
import { SettingsProviderProps } from "../types";
import { getMaximumDismissibleRiskLevel, RiskLevel } from "@/lib/risk-level";
import { getFrontendOptions } from "@/services/frontend-options-service";
import { SettingsContext } from "./settings-context";

export const SettingsProvider = (props: SettingsProviderProps) => {
  const [maxDismissibleRiskLevel, setMaxDismissibleRiskLevel] =
    useState<RiskLevel>(RiskLevel.Low);

  useEffect(() => {
    getFrontendOptions()
      .then((result) => {
        setMaxDismissibleRiskLevel(
          getMaximumDismissibleRiskLevel(result?.maxDismissibleRiskLevel),
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
