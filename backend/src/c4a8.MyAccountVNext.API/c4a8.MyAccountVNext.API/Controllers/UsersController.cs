using c4a8.MyAccountVNext.API.Models.Responses;
using c4a8.MyAccountVNext.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;

        public UsersController(GraphServiceClient graphClient)
        {
            _graphServiceClient = graphClient;
        }

        [HttpGet("me/riskstate")]
        public async Task<ActionResult<GenerateTapResponse>> GetRiskState()
        {
            var userId = User.GetObjectId();
            RiskyUser? riskyUser;
            try
            {
                riskyUser = await _graphServiceClient.IdentityProtection.RiskyUsers[userId].GetAsync();
            }
            catch (ODataError e)
            {
                if(e.ResponseStatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                throw;
            }

            var riskState = RiskState.None;

            if (riskyUser?.RiskState != null && riskyUser.RiskState.HasValue)
            {
                riskState = riskyUser.RiskState.Value;
            }

            return Ok(new GetRiskStateResponse(riskState));
        }
    }
}
