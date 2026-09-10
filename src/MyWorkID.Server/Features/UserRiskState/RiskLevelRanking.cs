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
                RiskLevel.Low => 1,
                RiskLevel.Medium => 2,
                RiskLevel.High => 3,
                _ => 0,
            };

        /// <summary>
        /// Returns <c>true</c> if a user with <paramref name="currentRiskLevel"/> is allowed to
        /// dismiss their own risk given the configured <paramref name="maxDismissibleRiskLevel"/>.
        /// Users with no assigned risk level (<c>null</c>, <see cref="RiskLevel.None"/>,
        /// <see cref="RiskLevel.Hidden"/>, or <see cref="RiskLevel.UnknownFutureValue"/>) are
        /// always allowed to confirm their account is safe.
        /// </summary>
        public static bool CanDismiss(RiskLevel? currentRiskLevel, RiskLevel maxDismissibleRiskLevel)
        {
            if (currentRiskLevel is null)
            {
                return true;
            }
            int currentRank = Rank(currentRiskLevel.Value);
            if (currentRank == 0)
            {
                return true;
            }
            return currentRank <= Rank(maxDismissibleRiskLevel);
        }
    }
}
