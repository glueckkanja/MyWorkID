using System.ComponentModel.DataAnnotations;
using Microsoft.Graph.Models;

namespace MyWorkID.Server.Options
{
    /// <summary>
    /// Configuration options for the user risk state feature.
    /// </summary>
    public class UserRiskStateOptions : BaseOptions
    {
        public const string SectionName = "UserRiskState";

        /// <summary>
        /// The maximum risk level a user is allowed to dismiss themselves. Users whose current
        /// risk level is higher than this value cannot dismiss their risk via self-service.
        /// Ordering (from lowest to highest): <see cref="RiskLevel.Low"/>, <see cref="RiskLevel.Medium"/>, <see cref="RiskLevel.High"/>.
        /// Defaults to <see cref="RiskLevel.Low"/>.
        /// </summary>
        [AllowedValues(RiskLevel.Low, RiskLevel.Medium, RiskLevel.High, ErrorMessage = "MaxDismissibleRiskLevel must be one of: Low, Medium, High.")]
        public RiskLevel MaxDismissibleRiskLevel { get; set; } = RiskLevel.Low;
    }
}
