using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.ResetPassword.Entities;
using c4a8.MyWorkID.Server.Features.ResetPassword.Filters;
using c4a8.MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.ResetPassword.Commands
{
    /// <summary>
    /// Handles password reset requests for a user.
    /// </summary>
    public class ResetPassword : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for resetting a user's password.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/resetPassword", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckResetPasswordAppConfigurationEndpointFilter>()
                .AddEndpointFilter<ResetPasswordAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .AddEndpointFilter<PasswordValidationFilter>()
                .WithTags(Strings.RESET_PASSWORD_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to reset a user's password.
        /// </summary>
        /// <param name="passwordResetRequest">The password reset request containing the new password.</param>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="graphClient">The Graph service client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success of the password reset operation.</returns>
        [Authorize(Roles = Strings.RESET_PASSWORD_ROLE)]
        public static async Task<IResult> HandleAsync([FromBody] PasswordResetRequest passwordResetRequest,
            ClaimsPrincipal user, GraphServiceClient graphClient, CancellationToken cancellationToken)
        {
            var userId = user.GetObjectId();
            await graphClient.Users[userId].PatchAsync(
                new User
                {
                    PasswordProfile = new PasswordProfile
                    {
                        Password = passwordResetRequest.NewPassword,
                        ForceChangePasswordNextSignIn = false
                    }
                },
                cancellationToken: cancellationToken);
            return TypedResults.Ok();
        }
    }
}
