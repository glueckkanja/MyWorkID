using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.UserRiskState.Entities;
using c4a8.MyWorkID.Server.Filters;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.UserRiskState.Queries
{
    /// <summary>
    /// This Endpoint returns the risk state and level of the user.
    /// Risk state and level are only returned if the RiskState is atRisk or ConfirmedCompromised.
    /// </summary>
    public class GetUserRiskState : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for getting the user's risk state.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<GetRiskStateResponse>("/api/me/riskstate", HandleAsync)
            .WithTags(Strings.USERRISKSTATE_OPENAPI_TAG)
                .RequireAuthorization()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>();
        }

        /// <summary>
        /// Handles the request to get the user's risk state.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="graphClient">The Graph service client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result containing the user's risk state and risk level.</returns>
        public static async Task<IResult> HandleAsync(ClaimsPrincipal user,
            GraphServiceClient graphClient, CancellationToken cancellationToken)
        {
            var userId = user.GetObjectId();
            RiskyUser? riskyUser;
            try
            {
                riskyUser = await graphClient.IdentityProtection.RiskyUsers[userId].GetAsync(cancellationToken: cancellationToken);
            }
            catch (ODataError e)
            {
                if (e.ResponseStatusCode == StatusCodes.Status404NotFound)
                {
                    return TypedResults.NotFound();
                }
                throw;
            }

            if (riskyUser == null)
            {
                return TypedResults.NotFound();
            }

            RiskLevel? riskLevel = null;
            RiskState riskState = riskyUser.RiskState ?? RiskState.None;

            if (riskyUser.RiskState == RiskState.AtRisk || riskyUser.RiskState == RiskState.ConfirmedCompromised)
            {
                riskLevel = riskyUser.RiskLevel;
            }

            return TypedResults.Ok(new GetRiskStateResponse(riskState, riskLevel));
        }

    }
}
