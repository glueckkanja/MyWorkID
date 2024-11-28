using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.Features.GenerateTap.Entities;
using c4a8.MyAccountVNext.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.GenerateTap.Commands
{
    public class GenerateTap : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/generatetap", HandleAsync)
                .WithTags(nameof(GenerateTap))
                .RequireAuthorization()
                .AddEndpointFilter<GenerateTapAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForUserIdEndpointFilter>();
        }

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
