using MyWorkID.Server.Common;
using MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;

namespace MyWorkID.Server.Features.ResetPassword.Queries
{
    /// <summary>
    /// Handles the check for the reset password auth context.
    /// </summary>
    public class CheckClaim : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for checking the reset password auth context.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<IResult>("/api/me/resetPassword/checkClaim", Handle)
                .RequireAuthorization()
                .AddEndpointFilter<CheckResetPasswordAppConfigurationEndpointFilter>()
                .AddEndpointFilter<ResetPasswordAuthContextEndpointFilter>()
                .WithTags(Strings.RESET_PASSWORD_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to check if the user has the reset password auth context.
        /// </summary>
        /// <returns>A result indicating the presence of the reset password auth context.</returns>
        [Authorize(Roles = Strings.RESET_PASSWORD_ROLE)]
        public static IResult Handle()
        {
            return TypedResults.NoContent();
        }
    }
}