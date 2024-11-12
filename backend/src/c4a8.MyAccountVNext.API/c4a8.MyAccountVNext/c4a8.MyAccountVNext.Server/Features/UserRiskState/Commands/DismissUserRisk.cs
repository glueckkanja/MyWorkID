using c4a8.MyAccountVNext.Server.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.UserRiskState.Commands
{
    public class DismissUserRisk : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/riskstate/dismiss", HandleAsync)
                .WithTags(Strings.USERRISKSTATE_OPENAPI_TAG);
        }

        [Authorize(Roles = "MyAccount.VNext.DismissUserRisk")]
        public static async Task<IResult> HandleAsync(ClaimsPrincipal user, HttpContext context, GraphServiceClient graphClient,
            IAuthContextService authContextService, CancellationToken cancellationToken)
        {

            string? claimsChallenge = authContextService.CheckForRequiredAuthContext(context, AppFunctions.DismissUserRisk);
            string? missingAuthContextId = authContextService.GetAuthContextId(AppFunctions.DismissUserRisk);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = user.GetObjectId();
                if (userId == null)
                {
                    return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
                }
                await graphClient.IdentityProtection.RiskyUsers.Dismiss.PostAsync(new Microsoft.Graph.IdentityProtection.RiskyUsers.Dismiss.DismissPostRequestBody() { UserIds = new List<string> { userId } });
                return TypedResults.Ok();
            }
            await authContextService.AddClaimsChallengeHeader(context, missingAuthContextId);
            return TypedResults.Unauthorized();
        }
    }
}
