using Microsoft.Extensions.Options;
using MyWorkID.Server.Common;
using MyWorkID.Server.Features.Configuration.Entities;
using MyWorkID.Server.Options;

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
            endpoints
                .MapGetWithOpenApi<FrontendOptions>("/api/config/frontend", Handle)
                .WithTags(nameof(GetFrontendConfig));
        }

        /// <summary>
        /// Handles the request to get frontend configuration settings.
        /// </summary>
        /// <param name="frontendOptions">The frontend options.</param>
        /// <param name="userRiskStateOptions">The user risk state options.</param>
        /// <returns>The frontend configuration settings.</returns>
        public static IResult Handle(
            IOptions<FrontendOptions> frontendOptions,
            IOptions<UserRiskStateOptions> userRiskStateOptions
        )
        {
            return TypedResults.Ok(
                new GetFrontendConfigResponse(frontendOptions.Value, userRiskStateOptions.Value)
            );
        }
    }
}
