using Microsoft.Graph.Models;

namespace c4a8.MyAccountVNext.API.Models.Responses
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
