using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using MyWorkID.Server.Common;
using MyWorkID.Server.Filters;

namespace MyWorkID.Server.Features.GenerateTap.Commands
{
    public class RevokeTemporaryAccessPass : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapDeleteWithOpenApi("api/me/tap/{temporaryAccessPassId}", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckGenerateTapAppConfigurationEndpointFilter>()
                .AddEndpointFilter<GenerateTapAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags("GenerateTap");
        }

        [Authorize(Roles = Strings.CREATE_TAP_ROLE)]
        public static async Task<IResult> HandleAsync(
            string temporaryAccessPassId,
            ClaimsPrincipal user,
            GraphServiceClient graphClient,
            CancellationToken cancellationToken
        )
        {
            var userId = user.GetObjectId();
            try
            {
                await graphClient
                    .Users[userId]
                    .Authentication.TemporaryAccessPassMethods[temporaryAccessPassId]
                    .DeleteAsync(cancellationToken: cancellationToken);
            }
            catch
            {
                return TypedResults.Problem(
                    detail: Strings.ERROR_UNABLE_TO_REVOKE_TEMPORARY_ACCESS_PASS,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            return TypedResults.NoContent();
        }
    }
}
