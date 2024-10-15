using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.Features.GenerateTap.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.GenerateTap.Commands
{
    public class GenerateTap : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPutWithOpenApi("api/me/generatetap", HandleAsync)
                .WithTags(nameof(GenerateTap));
        }

        [Authorize(Roles = "MyAccount.VNext.CreateTAP")]
        public static async Task<IResult> HandleAsync(ClaimsPrincipal user, HttpContext context, GraphServiceClient graphClient,
            IAuthContextService authContextService, CancellationToken cancellationToken)
        {
            string? claimsChallenge = authContextService.CheckForRequiredAuthContext(context, AppFunctions.GenerateTap);
            string? missingAuthContextId = authContextService.GetAuthContextId(AppFunctions.GenerateTap);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = user.GetObjectId();
                if (userId == null)
                {
                    return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
                }
                var getUser = await graphClient.Users[userId].GetAsync();
                var tapResponse = await graphClient.Users[userId].Authentication.TemporaryAccessPassMethods.PostAsync(new TemporaryAccessPassAuthenticationMethod());
                if (tapResponse?.TemporaryAccessPass == null)
                {
                    return TypedResults.Problem(detail: "Unable to generate TAP", statusCode: 500);
                }
                return TypedResults.Ok(new GenerateTapResponse(tapResponse.TemporaryAccessPass));
            }
            await authContextService.AddClaimsChallengeHeader(context, missingAuthContextId);
            //return TypedResults.Unauthorized();
            return TypedResults.Problem(detail: authContextService.GetClaimsChallengeMessage(), statusCode: 401);
        }
    }
}
