using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using MyWorkID.Server.Common;
using MyWorkID.Server.Exceptions;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Filters;

namespace MyWorkID.Server.Features.VerifiedId.Commands
{
    /// <summary>
    /// Discards a verification that was started for a specific action, e.g. because the user left that
    /// action without using it. Only relevant when every action requires its own verification.
    /// </summary>
    public class DiscardVerification : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for discarding a verification.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPostWithUpdatedOpenApi("api/me/verifiedId/discard", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to discard a verification.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="verifiedIdRequirementService">The service that manages the verification state.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="action">The action the verification to discard was started for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content, also when there was nothing to discard.</returns>
        [Authorize(Roles = Strings.VALIDATE_IDENTITY_ROLE)]
        public static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IVerifiedIdRequirementService verifiedIdRequirementService,
            ILogger<DiscardVerification> logger,
            [FromQuery] string? action,
            CancellationToken cancellationToken
        )
        {
            if (!VerificationAttributeValue.TryParseAction(action, out VerifiedIdAction verifiedIdAction))
            {
                return TypedResults.Problem(
                    Strings.ERROR_VERIFIED_ID_UNKNOWN_ACTION,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            try
            {
                await verifiedIdRequirementService.DiscardVerificationAsync(
                    user.GetObjectId()!,
                    verifiedIdAction,
                    cancellationToken
                );
            }
            catch (ConfigurationMissingException configurationMissingException)
            {
                logger.LogError(
                    configurationMissingException,
                    "Verified ID requirement is enabled but {ConfigurationKey} is not configured.",
                    configurationMissingException.ConfigurationKey
                );
                return TypedResults.Problem(
                    Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID_REQUIREMENT,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            return TypedResults.NoContent();
        }
    }
}
