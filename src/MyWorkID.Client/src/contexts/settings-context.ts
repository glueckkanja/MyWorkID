import React from "react";
import type { Settings } from "../types";
import { RiskLevel } from "@/lib/risk-level";

export const SettingsContext = React.createContext<Settings>({
  maxDismissibleRiskLevel: RiskLevel.Low,
});
