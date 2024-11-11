using c4a8.MyAccountVNext.Server.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId.Commands
{
    public class ValidateIdentity : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPostWithCreatedOpenApi("api/me/verifiedId/verify", HandleAsync)
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
        }

        [Authorize(Roles = "MyAccount.VNext.ValidateIdentity")]
        public static async Task<IResult> HandleAsync(VerifiedIdService verifiedIdService, ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var userId = user.GetObjectId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }

            var response = await verifiedIdService.CreatePresentationRequest(userId);
            return TypedResults.Created(string.Empty, response);
        }
    }
}
