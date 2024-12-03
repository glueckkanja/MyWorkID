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
    public class ResetPassword : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/resetPassword", HandleAsync)
                .WithTags(Strings.RESET_PASSWORD_OPENAPI_TAG)
                .RequireAuthorization()
                .AddEndpointFilter<ResetPasswordAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .AddEndpointFilter<PasswordValidationFilter>();
        }

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
