import { useMemo } from "react";
import { SettingsProviderProps } from "../types";
import { SettingsContext } from "./settings-context";

export const SettingsProvider = ({
  children,
  maxDismissibleRiskLevel,
}: SettingsProviderProps) => {
  const settingsValue = useMemo(
    () => ({ maxDismissibleRiskLevel }),
    [maxDismissibleRiskLevel],
  );

  return (
    <SettingsContext.Provider value={settingsValue}>
      {children}
    </SettingsContext.Provider>
  );
};
