import { RiskLevel } from "../types";

/**
 * Returns true if a user with `currentRiskLevel` is allowed to dismiss their own risk
 * given the configured `maxDismissibleRiskLevel`. Users with None/Unknown are always
 * allowed to confirm-safe.
 */
export const canDismissRiskLevel = (
  currentRiskLevel: RiskLevel,
  maxDismissibleRiskLevel: RiskLevel,
): boolean => currentRiskLevel <= maxDismissibleRiskLevel;

/**
 * Returns a human-readable label for a risk level (e.g. "Low", "Medium", "High").
 */
export const formatRiskLevel = (riskLevel: RiskLevel): string => {
  switch (riskLevel) {
    case RiskLevel.High:
      return "High";
    case RiskLevel.Medium:
      return "Medium";
    case RiskLevel.Low:
      return "Low";
    case RiskLevel.None:
      return "None";
    default:
      return "Unknown";
  }
};
