namespace c4a8.MyAccountVNext.API.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //[Authorize(Roles = "MyAccount.VNext.DismissUserRisk")]
    //public class DismissUserRiskController : ControllerBase
    //{
    //    private readonly GraphServiceClient _graphServiceClient;
    //    private readonly IAuthContextService _authContextService;

    //    public DismissUserRiskController(GraphServiceClient graphClient, IAuthContextService authContextService)
    //    {
    //        _graphServiceClient = graphClient;
    //        _authContextService = authContextService;
    //    }

    //    [HttpPut]
    //    public async Task<IActionResult> Get()
    //    {
    //        string? claimsChallenge = _authContextService.CheckForRequiredAuthContext(HttpContext, AppFunctions.DismissUserRisk);
    //        string? missingAuthContextId = _authContextService.GetAuthContextId(AppFunctions.DismissUserRisk);
    //        if (string.IsNullOrWhiteSpace(claimsChallenge))
    //        {
    //            var userId = User.GetObjectId();
    //            if (userId == null)
    //            {
    //                return StatusCode(StatusCodes.Status412PreconditionFailed, "UserId not provided");
    //            }
    //            await _graphServiceClient.IdentityProtection.RiskyUsers.Dismiss.PostAsync(new Microsoft.Graph.IdentityProtection.RiskyUsers.Dismiss.DismissPostRequestBody() { UserIds = new List<string> { userId } });
    //            return Ok();
    //        }
    //        await _authContextService.AddClaimsChallengeHeader(HttpContext, missingAuthContextId);
    //        return Unauthorized(_authContextService.GetClaimsChallengeMessage());
    //    }
    //}
}
