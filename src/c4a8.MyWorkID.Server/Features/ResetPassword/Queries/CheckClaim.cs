using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;

namespace c4a8.MyWorkID.Server.Features.ResetPassword.Queries
{
    public class CheckClaim : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<IResult>("/api/me/resetPassword/checkClaim", Handle)
                .WithTags(Strings.RESET_PASSWORD_OPENAPI_TAG)
                .RequireAuthorization()
                .AddEndpointFilter<ResetPasswordAuthContextEndpointFilter>();
        }

        [Authorize(Roles = Strings.RESET_PASSWORD_ROLE)]
        public static IResult Handle()
        {
            return TypedResults.NoContent();
        }
    }
}