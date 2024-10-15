using c4a8.MyAccountVNext.Server.Common;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.Configuration.Queries
{
    public class GetFrontendConfig : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<FrontendOptions>("/api/config/frontend", HandleAsync)
            .WithTags(nameof(GetFrontendConfig));
        }

        public static async Task<IResult> HandleAsync(ClaimsPrincipal user, IOptions<FrontendOptions> frontendOptions,
            CancellationToken cancellationToken)
        {
            return TypedResults.Ok(frontendOptions.Value);
        }
    }
}
