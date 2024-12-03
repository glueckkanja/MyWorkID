using Microsoft.Graph.Models;

namespace c4a8.MyWorkID.Server.Features.UserRiskState.Entities
{
    /// <summary>
    /// Represents the response containing the user's risk state and risk level.
    /// </summary>
    public class GetRiskStateResponse
    {
        /// <summary>
        /// Gets or sets the risk state of the user.
        /// </summary>
        public RiskState RiskState { get; set; }

        /// <summary>
        /// Gets or sets the risk level of the user.
        /// </summary>
        public RiskLevel? RiskLevel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRiskStateResponse"/> class.
        /// </summary>
        /// <param name="riskState">The risk state of the user.</param>
        /// <param name="riskLevel">The risk level of the user.</param>
        public GetRiskStateResponse(RiskState riskState, RiskLevel? riskLevel)
        {
            RiskState = riskState;
            RiskLevel = riskLevel;
        }
    }
}
