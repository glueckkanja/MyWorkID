using MyWorkID.Server.Common;
using MyWorkID.Server.Features.GenerateTap.Entities;
using MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace MyWorkID.Server.Features.GenerateTap.Commands
{
    /// <summary>
    /// Handles the generation of a Temporary Access Pass (TAP) for a user.
    /// </summary>
    public class GenerateTap : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for generating a TAP.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/generatetap", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckGenerateTapAppConfigurationEndpointFilter>()
                .AddEndpointFilter<GenerateTapAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags(nameof(GenerateTap));
        }

        /// <summary>
        /// Handles the request to generate a TAP.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="graphClient">The Graph service client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The generated TAP or an error response.</returns>
        [Authorize(Roles = Strings.CREATE_TAP_ROLE)]
        public static async Task<IResult> HandleAsync(ClaimsPrincipal user, GraphServiceClient graphClient,
            CancellationToken cancellationToken)
        {
            var userId = user.GetObjectId();
            var tapResponse = await graphClient.Users[userId].Authentication.TemporaryAccessPassMethods.PostAsync(
                new TemporaryAccessPassAuthenticationMethod(),
                cancellationToken: cancellationToken);
            if (tapResponse?.TemporaryAccessPass == null)
            {
                return TypedResults.Problem(detail: Strings.ERROR_UNABLE_TO_GENERATE_TAP, statusCode: StatusCodes.Status500InternalServerError);
            }
            return TypedResults.Ok(new GenerateTapResponse(tapResponse.TemporaryAccessPass));
        }
    }
}
