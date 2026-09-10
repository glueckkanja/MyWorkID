using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web;
using MyWorkID.Server.Common;
using MyWorkID.Server.Filters;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Features.UserRiskState.Commands
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
            endpoints
                .MapPutWithOpenApi("api/me/riskstate/dismiss", HandleAsync)
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
        /// <param name="userRiskStateOptions">The user risk state configuration options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success of the dismissal operation.</returns>
        [Authorize(Roles = Strings.DISMISS_USER_RISK_ROLE)]
        public static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            GraphServiceClient graphClient,
            IOptions<UserRiskStateOptions> userRiskStateOptions,
            CancellationToken cancellationToken
        )
        {
            string? userId = user.GetObjectId();
            RiskLevel maxDismissibleRiskLevel = userRiskStateOptions.Value.MaxDismissibleRiskLevel;

            RiskyUser? riskyUser = null;
            try
            {
                riskyUser = await graphClient
                    .IdentityProtection.RiskyUsers[userId]
                    .GetAsync(cancellationToken: cancellationToken);
            }
            catch (ODataError odataError) when (odataError.ResponseStatusCode == StatusCodes.Status404NotFound)
            {
                // No risky-user record means there is nothing above the max level; allow dismiss to run for confirm-safe semantics.
            }

            RiskLevel? currentRiskLevel = riskyUser?.RiskLevel;
            if (!RiskLevelRanking.CanDismiss(currentRiskLevel, maxDismissibleRiskLevel))
            {
                return TypedResults.Problem(
                    Strings.ERROR_RISK_LEVEL_EXCEEDS_MAX_DISMISSIBLE,
                    statusCode: StatusCodes.Status403Forbidden
                );
            }

            await graphClient.IdentityProtection.RiskyUsers.Dismiss.PostAsync(
                new Microsoft.Graph.IdentityProtection.RiskyUsers.Dismiss.DismissPostRequestBody()
                {
                    UserIds = [userId!],
                },
                cancellationToken: cancellationToken
            );
            return TypedResults.Ok();
        }
    }
}
