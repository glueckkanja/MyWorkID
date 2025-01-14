using MyWorkID.Server.Common;
using MyWorkID.Server.Features.VerifiedId.Exceptions;
using MyWorkID.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;
using Microsoft.Graph.Models.ODataErrors;
using System.Linq;

namespace MyWorkID.Server.Features.VerifiedId.Commands
{
    /// <summary>
    /// Handles the creation of a QR code for verified ids.
    /// </summary>
    public class ValidateIdentity : IEndpoint
    {
        private const string GRAPH_VERIFIED_ID_LICENSE_ERROR_MESSAGE = "Premium features cannot be used until billing is enabled by the admin.";
        private const string LICENSE_MISSING_PROBLEM_DETAIL = "License missing.";

        /// <summary>
        /// Maps the endpoint for the creation of a QR code for verified ids.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPostWithCreatedOpenApi("api/me/verifiedId/verify", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckVerifiedIdAppConfigurationEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
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
            catch (ODataError e)
            {
                if (e.Message.Contains(GRAPH_VERIFIED_ID_LICENSE_ERROR_MESSAGE, StringComparison.OrdinalIgnoreCase))
                {
                    return TypedResults.Problem(LICENSE_MISSING_PROBLEM_DETAIL);
                }
                throw;
            }
        }
    }
}