using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Graph.Users.Item.RevokeSignInSessions;
using Microsoft.Identity.Web;
using MyWorkID.Server.Common;
using MyWorkID.Server.Filters;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Features.UserRiskState.Commands
{
    /// <summary>
    /// Handles the dismissal of user risk state and revokes all sign-in sessions of the user afterwards.
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
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success of the dismissal operation.</returns>
        [Authorize(Roles = Strings.DISMISS_USER_RISK_ROLE)]
        public static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            GraphServiceClient graphClient,
            IOptions<UserRiskStateOptions> userRiskStateOptions,
            ILogger<DismissUserRisk> logger,
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
            catch (ODataError odataError)
                when (odataError.ResponseStatusCode == StatusCodes.Status404NotFound)
            {
                return TypedResults.Problem(
                    $"No risky user record found for user {userId}. User can proceed with risk dismissal for confirm-safe semantics.",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            // UnknownFutureValue is a sentinel for an active record with an absent RiskLevel.
            // RiskLevelRanking.Rank maps it to int.MaxValue, so CanDismiss always denies it.
            RiskLevel? currentRiskLevel = riskyUser
                is {
                    RiskState: RiskState.AtRisk or RiskState.ConfirmedCompromised,
                    RiskLevel: null
                }
                ? RiskLevel.UnknownFutureValue
                : riskyUser?.RiskLevel;
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

            // Invalidate refresh tokens and session cookies so that sessions established while the user was at risk cannot continue.
            RevokeSignInSessionsPostResponse? revokeSignInSessionsResponse = await graphClient
                .Users[userId]
                .RevokeSignInSessions.PostAsRevokeSignInSessionsPostResponseAsync(
                    cancellationToken: cancellationToken
                );
            if (revokeSignInSessionsResponse?.Value != true)
            {
                logger.LogError(
                    "Risk was dismissed for user {UserId} but revoking sign-in sessions did not report success.",
                    userId
                );
                return TypedResults.Problem(
                    Strings.ERROR_REVOKE_SIGN_IN_SESSIONS_FAILED,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            return TypedResults.Ok();
        }
    }
}
