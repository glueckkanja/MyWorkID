using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.UserRiskState.Commands
{
    public class DismissUserRisk : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/riskstate/dismiss", HandleAsync)
                .WithTags(Strings.USERRISKSTATE_OPENAPI_TAG)
                .RequireAuthorization()
                .AddEndpointFilter<DismissUserRiskAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>();
        }

        [Authorize(Roles = Strings.DISMISS_USER_RISK_ROLE)]
        public static async Task<IResult> HandleAsync(ClaimsPrincipal user,
            GraphServiceClient graphClient, CancellationToken cancellationToken)
        {
            var userId = user.GetObjectId();
            await graphClient.IdentityProtection.RiskyUsers.Dismiss.PostAsync(
                new Microsoft.Graph.IdentityProtection.RiskyUsers.Dismiss.DismissPostRequestBody()
                {
                    UserIds = [userId!]
                },
                cancellationToken: cancellationToken);
            return TypedResults.Ok();
        }
    }
}
