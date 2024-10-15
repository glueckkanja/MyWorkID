using c4a8.MyAccountVNext.Server.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.ResetPassword.Queries
{
    public class CheckClaim : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<IResult>("/api/me/resetPassword/checkClaim", HandleAsync)
                .WithTags(Strings.RESETPASSWORD_OPENAPI_TAG);
        }

        [Authorize(Roles = "MyAccount.VNext.PasswordReset")]
        public static async Task<IResult> HandleAsync(ClaimsPrincipal user, HttpContext context,
            IAuthContextService authContextService, CancellationToken cancellationToken)
        {
            string? claimsChallenge = authContextService.CheckForRequiredAuthContext(context, AppFunctions.ResetPassword);
            string? missingAuthContextId = authContextService.GetAuthContextId(AppFunctions.ResetPassword);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                return TypedResults.NoContent();
            }
            await authContextService.AddClaimsChallengeHeader(context, missingAuthContextId);
            return TypedResults.Unauthorized();
        }
    }
}