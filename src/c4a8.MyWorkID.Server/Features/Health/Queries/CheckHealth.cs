using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.Configuration;

namespace c4a8.MyWorkID.Server.Features.Health.Queries
{
    public class CheckHealth : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<FrontendOptions>("/api/general", HandleAsync)
            .WithTags(nameof(CheckHealth));
        }

        public static IResult HandleAsync()
        {
            return TypedResults.Ok("Healthy");
        }
    }
}
