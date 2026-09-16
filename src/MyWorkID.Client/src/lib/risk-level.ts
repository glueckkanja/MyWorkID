export enum RiskLevel {
  None = 0,
  Low = 1,
  Medium = 2,
  High = 3,
  Unknown = 999,
}

export enum RiskLabel {
  Unknown = "Unknown",
  None = "None",
  Low = "Low",
  Medium = "Medium",
  High = "High",
}

export const getRiskLevelFromLabel = (
  riskLevelLabel: string | undefined,
): RiskLevel | undefined => {
  switch (riskLevelLabel?.toLowerCase()) {
    case RiskLabel.High.toLowerCase():
      return RiskLevel.High;
    case RiskLabel.Medium.toLowerCase():
      return RiskLevel.Medium;
    case RiskLabel.Low.toLowerCase():
      return RiskLevel.Low;
    case RiskLabel.None.toLowerCase():
      return RiskLevel.None;
    default:
      return undefined;
  }
};

export const getMaximumDismissibleRiskLevel = (
  riskLevelLabel: string | undefined,
): RiskLevel => getRiskLevelFromLabel(riskLevelLabel) ?? RiskLevel.Low;

/**
 * Returns true if a user with `currentRiskLevel` is allowed to dismiss their own risk
 * given the configured `maxDismissibleRiskLevel`. Users with None are always allowed
 * to confirm-safe. Unknown risk is not dismissible because the current risk level could
 * not be resolved.
 */
export const canDismissRiskLevel = (
  currentRiskLevel: RiskLevel,
  maxDismissibleRiskLevel: RiskLevel,
): boolean =>
  currentRiskLevel !== RiskLevel.Unknown &&
  currentRiskLevel <= maxDismissibleRiskLevel;

/**
 * Returns a human-readable label for a risk level (e.g. "Low", "Medium", "High").
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
