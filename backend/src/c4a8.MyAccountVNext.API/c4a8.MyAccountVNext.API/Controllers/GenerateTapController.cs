using c4a8.MyAccountVNext.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "MyAccount.VNext.CreateTAP")]
    public class GenerateTapController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IAuthContextService _authContextService;

        public GenerateTapController(GraphServiceClient graphClient, IAuthContextService authContextService)
        {
            _graphServiceClient = graphClient;
            _authContextService = authContextService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GenerateTap()
        {
            string? claimsChallenge = _authContextService.CheckForRequiredAuthContext(HttpContext, AppFunctions.GenerateTap);
            string? missingAuthContextId = _authContextService.GetAuthContextId(AppFunctions.GenerateTap);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = User.GetObjectId();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, "UserId not provided");
                }
                var tapResponse = await _graphServiceClient.Users[userId].Authentication.TemporaryAccessPassMethods.PostAsync(new Microsoft.Graph.Models.TemporaryAccessPassAuthenticationMethod());
                return Ok(tapResponse.TemporaryAccessPass);
            }
            await _authContextService.AddClaimsChallengeHeader(HttpContext, missingAuthContextId);
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }
}
