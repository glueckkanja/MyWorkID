using Microsoft.Graph.Models;

namespace c4a8.MyAccountVNext.API.Models.Responses
{
    public class GetRiskStateResponse
    {
        public RiskState RiskState { get; set; }

        public GetRiskStateResponse(RiskState riskState)
        {
            RiskState = riskState;
        }
    }
}
