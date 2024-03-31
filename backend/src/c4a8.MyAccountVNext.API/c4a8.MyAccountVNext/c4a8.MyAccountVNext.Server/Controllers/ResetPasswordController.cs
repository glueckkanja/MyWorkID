using c4a8.MyAccountVNext.API.Services;
using c4a8.MyAccountVNext.Server.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MyAccount.VNext.PasswordReset")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IAuthContextService _authContextService;

        public ResetPasswordController(GraphServiceClient graphClient, IAuthContextService authContextService)
        {
            _graphServiceClient = graphClient;
            _authContextService = authContextService;
        }

        /// <summary>
        /// This endpoint sets the user's password.
        /// </summary>
        /// <returns></returns>
        [HttpPut("")]
        public async Task<ActionResult> ResetPassword([FromBody] PasswordResetRequest passwordResetRequest)
        {
            string? claimsChallenge = _authContextService.CheckForRequiredAuthContext(HttpContext, AppFunctions.ResetPassword);
            string? missingAuthContextId = _authContextService.GetAuthContextId(AppFunctions.ResetPassword);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = User.GetObjectId();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, "UserId not provided");
                }
                await _graphServiceClient.Users[userId].PatchAsync(
                    new User
                    {
                        PasswordProfile = new PasswordProfile
                        {
                            Password = passwordResetRequest.NewPassword,
                            ForceChangePasswordNextSignIn = false
                        }
                    });

                return Ok();

            }
            await _authContextService.AddClaimsChallengeHeader(HttpContext, missingAuthContextId);
            return Unauthorized(_authContextService.GetClaimsChallengeMessage());
        }
    }
}
