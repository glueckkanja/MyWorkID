using Microsoft.Graph.Models;

namespace MyWorkID.Server.Features.UserRiskState
{
    /// <summary>
    /// Ordering helper for <see cref="RiskLevel"/> values. The underlying enum values are not
    /// guaranteed to be ordered semantically, so we compare against an explicit ranking.
    /// </summary>
    public static class RiskLevelRanking
    {
        private static int Rank(RiskLevel level) =>
            level switch
            {
                RiskLevel.None => 0,
                RiskLevel.Low => 1,
                RiskLevel.Medium => 2,
                RiskLevel.High => 3,
                _ => int.MaxValue, // Hidden, UnknownFutureValue — unresolvable, always denied
            };

        /// <summary>
        /// Returns <c>true</c> if a user with <paramref name="currentRiskLevel"/> is allowed to
        /// dismiss their own risk given the configured <paramref name="maxDismissibleRiskLevel"/>.
        /// <c>null</c> and <see cref="RiskLevel.None"/> are always allowed (confirm-safe path).
        /// <see cref="RiskLevel.Hidden"/> and <see cref="RiskLevel.UnknownFutureValue"/> are always
        /// denied because the actual risk level cannot be determined.
        /// </summary>
        public static bool CanDismiss(RiskLevel? currentRiskLevel, RiskLevel maxDismissibleRiskLevel)
        {
            if (currentRiskLevel is null)
            {
                return true;
            }
            return Rank(currentRiskLevel.Value) <= Rank(maxDismissibleRiskLevel);
        }
    }
}
