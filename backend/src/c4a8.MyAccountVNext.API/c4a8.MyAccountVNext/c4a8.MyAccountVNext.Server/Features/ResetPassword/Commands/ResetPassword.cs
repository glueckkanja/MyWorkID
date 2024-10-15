using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.Features.ResetPassword.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.ResetPassword.Commands
{
    public class ResetPassword : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/resetPassword", HandleAsync)
                .WithTags(Strings.RESETPASSWORD_OPENAPI_TAG);
        }

        [Authorize(Roles = "MyAccount.VNext.PasswordReset")]
        public static async Task<IResult> HandleAsync([FromBody] PasswordResetRequest passwordResetRequest, ClaimsPrincipal user, HttpContext context, GraphServiceClient graphClient,
            IAuthContextService authContextService, CancellationToken cancellationToken)
        {
            string? claimsChallenge = authContextService.CheckForRequiredAuthContext(context, AppFunctions.ResetPassword);
            string? missingAuthContextId = authContextService.GetAuthContextId(AppFunctions.ResetPassword);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = user.GetObjectId();
                if (userId == null)
                {
                    return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
                }
                await graphClient.Users[userId].PatchAsync(
                    new User
                    {
                        PasswordProfile = new PasswordProfile
                        {
                            Password = passwordResetRequest.NewPassword,
                            ForceChangePasswordNextSignIn = false
                        }
                    });

                return TypedResults.Ok();

            }
            await authContextService.AddClaimsChallengeHeader(context, missingAuthContextId);
            return TypedResults.Unauthorized();
        }
    }
}
