using c4a8.MyWorkID.Server.Common;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.Configuration.Queries
{
    public class GetFrontendConfig : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<FrontendOptions>("/api/config/frontend", Handle)
            .WithTags(nameof(GetFrontendConfig));
        }

        public static IResult Handle(ClaimsPrincipal user, IOptions<FrontendOptions> frontendOptions,
            CancellationToken cancellationToken)
        {
            return TypedResults.Ok(frontendOptions.Value);
        }
    }
}
