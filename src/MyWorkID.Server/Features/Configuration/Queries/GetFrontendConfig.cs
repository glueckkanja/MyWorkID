using MyWorkID.Server.Common;
using MyWorkID.Server.Options;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MyWorkID.Server.Features.Configuration.Queries
{
    /// <summary>
    /// Handles the retrieval of frontend configuration settings.
    /// </summary>
    public class GetFrontendConfig : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for getting frontend configuration.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<FrontendOptions>("/api/config/frontend", Handle)
            .WithTags(nameof(GetFrontendConfig));
        }

        /// <summary>
        /// Handles the request to get frontend configuration settings.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="frontendOptions">The frontend options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The frontend configuration settings.</returns>
        public static IResult Handle(ClaimsPrincipal user, IOptions<FrontendOptions> frontendOptions,
            CancellationToken cancellationToken)
        {
            return TypedResults.Ok(frontendOptions.Value);
        }
    }
}
