using Microsoft.Graph.Models;
using MyWorkID.Server.Features.UserRiskState;

namespace MyWorkID.Server.UnitTests.Features.UserRiskState
{
    public class RiskLevelRankingTests
    {
        [Theory]
        // No current risk level: always allowed (confirm-safe path)
        [InlineData(null, RiskLevel.Low, true)]
        [InlineData(null, RiskLevel.Medium, true)]
        [InlineData(null, RiskLevel.High, true)]
        // Non-ranked values behave like "no risk"
        [InlineData(RiskLevel.None, RiskLevel.Low, true)]
        [InlineData(RiskLevel.Hidden, RiskLevel.Low, true)]
        [InlineData(RiskLevel.UnknownFutureValue, RiskLevel.Low, true)]
        // Max = Low: only Low is dismissible
        [InlineData(RiskLevel.Low, RiskLevel.Low, true)]
        [InlineData(RiskLevel.Medium, RiskLevel.Low, false)]
        [InlineData(RiskLevel.High, RiskLevel.Low, false)]
        // Max = Medium: Low and Medium are dismissible, High is not
        [InlineData(RiskLevel.Low, RiskLevel.Medium, true)]
        [InlineData(RiskLevel.Medium, RiskLevel.Medium, true)]
        [InlineData(RiskLevel.High, RiskLevel.Medium, false)]
        // Max = High: everything is dismissible
        [InlineData(RiskLevel.Low, RiskLevel.High, true)]
        [InlineData(RiskLevel.Medium, RiskLevel.High, true)]
        [InlineData(RiskLevel.High, RiskLevel.High, true)]
        public void CanDismiss_ReturnsExpectedResult(RiskLevel? currentRiskLevel, RiskLevel maxDismissibleRiskLevel, bool expected)
        {
            bool actual = RiskLevelRanking.CanDismiss(currentRiskLevel, maxDismissibleRiskLevel);
            Assert.Equal(expected, actual);
        }
    }
}
