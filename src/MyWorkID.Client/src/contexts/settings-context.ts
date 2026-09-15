import React from "react";
import { RiskLevel, Settings } from "../types";

export const SettingsContext = React.createContext<Settings>({
  maxDismissibleRiskLevel: RiskLevel.Low,
});
