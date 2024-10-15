using c4a8.MyAccountVNext.Server.Features.UserRiskState.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web;

namespace c4a8.MyAccountVNext.API.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //[Authorize]
    //public class UsersController : ControllerBase
    //{
    //    private readonly GraphServiceClient _graphServiceClient;

    //    public UsersController(GraphServiceClient graphClient)
    //    {
    //        _graphServiceClient = graphClient;
    //    }

    //    /// <summary>
    //    /// This Endpoint returns the risk state of the user.
    //    /// The RiskLevel is only returned if the RiskState is atRisk
    //    /// </summary>
    //    /// <returns></returns>
    //    [HttpGet("me/riskstate")]
    //    public async Task<ActionResult<GetRiskStateResponse>> GetRiskState()
    //    {
    //        var userId = User.GetObjectId();
    //        RiskyUser? riskyUser;
    //        try
    //        {
    //            riskyUser = await _graphServiceClient.IdentityProtection.RiskyUsers[userId].GetAsync();
    //        }
    //        catch (ODataError e)
    //        {
    //            if (e.ResponseStatusCode == StatusCodes.Status404NotFound)
    //            {
    //                return NotFound();
    //            }
    //            throw;
    //        }

    //        if (riskyUser == null)
    //        {
    //            Response.Headers.Add("x-error-details", "GraphAPI did not return with 404 but not RiskyUser was returned");
    //            return NotFound();
    //        }

    //        RiskLevel? riskLevel = null;
    //        RiskState riskState = RiskState.None;

    //        if (riskyUser.RiskState == RiskState.AtRisk || riskyUser.RiskState == RiskState.ConfirmedCompromised)
    //        {
    //            riskLevel = riskyUser.RiskLevel;
    //        }

    //        return Ok(new GetRiskStateResponse(riskState, riskLevel));
    //    }
    //}
}
