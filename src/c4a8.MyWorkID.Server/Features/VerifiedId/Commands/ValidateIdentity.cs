using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions;
using c4a8.MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.Commands
{
    /// <summary>
    /// Handles the creation of a QR code for verified ids.
    /// </summary>
    public class ValidateIdentity : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for the creation of a QR code for verified ids.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPostWithCreatedOpenApi("api/me/verifiedId/verify", HandleAsync)
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG)
                .RequireAuthorization()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>();
        }

        /// <summary>
        /// Handles the request to create a QR code for verified ids.
        /// </summary>
        /// <param name="verifiedIdService">The verified ID service.</param>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success or failure of the identity validation request.</returns>
        [Authorize(Roles = Strings.VALIDATE_IDENTITY_ROLE)]
        public static async Task<IResult> HandleAsync(VerifiedIdService verifiedIdService, ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var userId = user.GetObjectId();
            try
            {
                var response = await verifiedIdService.CreatePresentationRequest(userId!);
                return TypedResults.Created(string.Empty, response);
            }
            catch (CreatePresentationException)
            {
                return TypedResults.BadRequest();
            }
        }
    }
}
