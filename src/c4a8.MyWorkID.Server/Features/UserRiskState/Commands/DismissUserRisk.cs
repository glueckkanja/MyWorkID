using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.UserRiskState.Commands
{
    /// <summary>
    /// Handles the dismissal of user risk state.
    /// </summary>
    public class DismissUserRisk : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for dismissing the user risk state.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/riskstate/dismiss", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckDismissUserRiskAppConfigurationEndpointFilter>()
                .AddEndpointFilter<DismissUserRiskAuthContextEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags(Strings.USERRISKSTATE_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to dismiss the user risk state.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="graphClient">The Graph service client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success of the dismissal operation.</returns>
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
