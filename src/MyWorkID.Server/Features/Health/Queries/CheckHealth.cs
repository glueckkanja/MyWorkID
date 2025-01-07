using MyWorkID.Server.Common;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Features.Health.Queries
{
    /// <summary>
    /// Handles health check requests for the application.
    /// </summary>
    public class CheckHealth : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for checking the health of the application.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<FrontendOptions>("/api/general", HandleAsync)
            .WithTags(nameof(CheckHealth));
        }

        /// <summary>
        /// Handles the request to check the health of the application.
        /// </summary>
        /// <returns>A result indicating the health status of the application.</returns>
        public static IResult HandleAsync()
        {
            return TypedResults.Ok("Healthy");
        }
    }
}
