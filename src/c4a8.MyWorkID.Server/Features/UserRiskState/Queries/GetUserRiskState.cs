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
    /// This Endpoint returns the risk state of the user.
    /// The RiskLevel is only returned if the RiskState is atRisk
    /// </summary>
    public class GetUserRiskState : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGetWithOpenApi<GetRiskStateResponse>("/api/me/riskstate", HandleAsync)
            .WithTags(Strings.USERRISKSTATE_OPENAPI_TAG)
                .RequireAuthorization()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>();
        }

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
