import { RiskLevel } from "../types";

const RANK: Record<RiskLevel, number> = {
  [RiskLevel.Low]: 1,
  [RiskLevel.Medium]: 2,
  [RiskLevel.High]: 3,
  [RiskLevel.None]: 0,
  [RiskLevel.Unknown]: 0,
};

/**
 * Returns true if a user with `currentRiskLevel` is allowed to dismiss their own risk
 * given the configured `maxDismissibleRiskLevel`. Users with None/Unknown are always
 * allowed to confirm-safe.
 */
export const canDismissRiskLevel = (
  currentRiskLevel: RiskLevel,
  maxDismissibleRiskLevel: RiskLevel,
): boolean => {
  const currentRank = RANK[currentRiskLevel] ?? 0;
  if (currentRank === 0) {
    return true;
  }
  return currentRank <= (RANK[maxDismissibleRiskLevel] ?? 0);
};

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
