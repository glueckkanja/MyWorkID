using System.Security.Claims;
using Microsoft.Identity.Web;
using MyWorkID.Server.Common;
using MyWorkID.Server.Exceptions;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Filters;

namespace MyWorkID.Server.Features.VerifiedId.Queries
{
    /// <summary>
    /// Returns whether sensitive actions currently require a recent Verified ID verification and,
    /// if so, the verification state of the signed-in user. The frontend uses this to inform the
    /// user and to guide them to the identity validation before they trigger a gated action.
    /// </summary>
    public class GetVerifiedIdStatus : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for retrieving the Verified ID status of the signed-in user.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGetWithOpenApi<GetVerifiedIdStatusResponse>(
                    "/api/me/verifiedId/status",
                    HandleAsync
                )
                .RequireAuthorization()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to get the Verified ID status of the signed-in user.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="verifiedIdRequirementService">The service that evaluates the verification state.</param>
        /// <param name="timeProvider">The time provider used to report the server time.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Verified ID status of the user.</returns>
        public static async Task<IResult> HandleAsync(
            ClaimsPrincipal user,
            IVerifiedIdRequirementService verifiedIdRequirementService,
            TimeProvider timeProvider,
            ILogger<GetVerifiedIdStatus> logger,
            CancellationToken cancellationToken
        )
        {
            int validityInMinutes = verifiedIdRequirementService.ValidityInMinutes;

            if (!verifiedIdRequirementService.IsEnabled)
            {
                return TypedResults.Ok(
                    new GetVerifiedIdStatusResponse(
                        Required: false,
                        ValidityInMinutes: validityInMinutes,
                        IsVerified: false,
                        LastVerifiedAt: null,
                        ValidUntil: null,
                        ServerTime: timeProvider.GetUtcNow()
                    )
                );
            }

            string userId = user.GetObjectId()!;
            VerifiedIdVerificationState verificationState;
            try
            {
                verificationState = await verifiedIdRequirementService.GetVerificationStateAsync(
                    userId,
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

            return TypedResults.Ok(
                new GetVerifiedIdStatusResponse(
                    Required: true,
                    ValidityInMinutes: validityInMinutes,
                    IsVerified: verificationState.IsValid,
                    LastVerifiedAt: verificationState.LastVerifiedAt,
                    ValidUntil: verificationState.ValidUntil,
                    ServerTime: timeProvider.GetUtcNow()
                )
            );
        }
    }
}
