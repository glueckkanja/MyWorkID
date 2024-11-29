using Microsoft.Graph.Models;

namespace c4a8.MyWorkID.Server.Features.UserRiskState.Entities
{
    public class GetRiskStateResponse
    {
        public RiskState RiskState { get; set; }
        public RiskLevel? RiskLevel { get; set; }

        public GetRiskStateResponse(RiskState riskState, RiskLevel? riskLevel)
        {
            RiskState = riskState;
            RiskLevel = riskLevel;
        }
    }
}
