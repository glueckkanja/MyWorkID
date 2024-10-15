using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MyAccount.VNext.CreateTAP")]
    public class GenerateTapController : ControllerBase
    {
        //private readonly GraphServiceClient _graphServiceClient;
        //private readonly IAuthContextService _authContextService;

        //public GenerateTapController(GraphServiceClient graphClient, IAuthContextService authContextService)
        //{
        //    _graphServiceClient = graphClient;
        //    _authContextService = authContextService;
        //}

        //[HttpPut("")]
        //public async Task<ActionResult<GenerateTapResponse>> GenerateTap()
        //{
        //    string? claimsChallenge = _authContextService.CheckForRequiredAuthContext(HttpContext, AppFunctions.GenerateTap);
        //    string? missingAuthContextId = _authContextService.GetAuthContextId(AppFunctions.GenerateTap);
        //    if (string.IsNullOrWhiteSpace(claimsChallenge))
        //    {
        //        var userId = User.GetObjectId();
        //        if (userId == null)
        //        {
        //            return StatusCode(StatusCodes.Status412PreconditionFailed, "UserId not provided");
        //        }
        //        var tapResponse = await _graphServiceClient.Users[userId].Authentication.TemporaryAccessPassMethods.PostAsync(new Microsoft.Graph.Models.TemporaryAccessPassAuthenticationMethod());
        //        if (tapResponse?.TemporaryAccessPass == null)
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to generate TAP");
        //        }
        //        return Ok(new GenerateTapResponse(tapResponse.TemporaryAccessPass));
        //    }
        //    await _authContextService.AddClaimsChallengeHeader(HttpContext, missingAuthContextId);
        //    return Unauthorized(_authContextService.GetClaimsChallengeMessage());
        //}
    }
}
