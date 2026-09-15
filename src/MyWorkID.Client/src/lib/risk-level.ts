import { RiskLabel, RiskLevel } from "../types";

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
 * Returns a human-readable label for a risk level (e.g. "low", "medium", "high").
 */
export const getRiskLabelFromLevel = (riskLevel: RiskLevel): RiskLabel => {
  switch (riskLevel) {
    case RiskLevel.High:
      return RiskLabel.High;
    case RiskLevel.Medium:
      return RiskLabel.Medium;
    case RiskLevel.Low:
      return RiskLabel.Low;
    case RiskLevel.None:
      return RiskLabel.None;
    default:
      return RiskLabel.Unknown;
  }
};
