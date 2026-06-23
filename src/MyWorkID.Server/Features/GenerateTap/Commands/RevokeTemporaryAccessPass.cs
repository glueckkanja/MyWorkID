using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
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
            string? userId = user.GetObjectId();
            try
            {
                await graphClient
                    .Users[userId]
                    .Authentication.TemporaryAccessPassMethods[temporaryAccessPassId]
                    .DeleteAsync(cancellationToken: cancellationToken);
            }
            catch (ODataError e)
            {
                if (e.ResponseStatusCode == StatusCodes.Status404NotFound)
                {
                    return TypedResults.NotFound();
                }
                if (e.ResponseStatusCode == StatusCodes.Status400BadRequest)
                {
                    return TypedResults.ValidationProblem(
                        new Dictionary<string, string[]>
                        {
                            [nameof(temporaryAccessPassId)] = [e.Message],
                        }
                    );
                }
                throw;
            }
            return TypedResults.NoContent();
        }
    }
}
