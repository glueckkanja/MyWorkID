using Microsoft.Identity.Web;
using MyWorkID.Server.Exceptions;
using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.Filters
{
    /// <summary>
    /// Endpoint filter that blocks sensitive actions unless the user completed a Verified ID
    /// verification within the configured validity window. The filter is a no-op when the
    /// <c>VerifiedIdRequirement</c> feature is disabled, which keeps existing deployments unaffected.
    /// When every action requires its own verification, the verification must be bound to the action of the
    /// endpoint (see <see cref="RequireRecentVerifiedIdExtensions.RequireRecentVerifiedId"/>) and is claimed
    /// before the action runs, so it unlocks a single action even for overlapping requests. The claim is given
    /// back if the action fails.
    /// </summary>
    public class RequireRecentVerifiedIdEndpointFilter : IEndpointFilter
    {
        private readonly IVerifiedIdRequirementService _verifiedIdRequirementService;
        private readonly ILogger<RequireRecentVerifiedIdEndpointFilter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireRecentVerifiedIdEndpointFilter"/> class.
        /// </summary>
        /// <param name="verifiedIdRequirementService">The service that evaluates the verification state.</param>
        /// <param name="logger">The logger instance.</param>
        public RequireRecentVerifiedIdEndpointFilter(
            IVerifiedIdRequirementService verifiedIdRequirementService,
            ILogger<RequireRecentVerifiedIdEndpointFilter> logger
        )
        {
            _verifiedIdRequirementService = verifiedIdRequirementService;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the endpoint filter asynchronously.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context.</param>
        /// <param name="next">The next delegate to invoke.</param>
        /// <returns>The result of the filter invocation.</returns>
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next
        )
        {
            if (!_verifiedIdRequirementService.IsEnabled)
            {
                return await next(context);
            }

            string? userId = context.HttpContext.User.GetObjectId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            VerifiedIdVerificationState verificationState;
            try
            {
                verificationState = await _verifiedIdRequirementService.GetVerificationStateAsync(
                    userId,
                    context.HttpContext.RequestAborted
                );
            }
            catch (ConfigurationMissingException configurationMissingException)
            {
                _logger.LogError(
                    configurationMissingException,
                    "Verified ID requirement is enabled but {ConfigurationKey} is not configured.",
                    configurationMissingException.ConfigurationKey
                );
                return Results.Problem(
                    Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID_REQUIREMENT,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

            if (!IsVerificationAcceptedForEndpoint(verificationState, context.HttpContext))
            {
                return CreateVerificationMissingResult(verificationState);
            }

            if (!_verifiedIdRequirementService.RequiresVerificationPerAction)
            {
                return await next(context);
            }

            // The verification is claimed before the action runs, so overlapping requests cannot use it twice.
            VerifiedIdVerificationClaim? claim =
                await _verifiedIdRequirementService.TryClaimVerificationAsync(
                    userId,
                    verificationState,
                    context.HttpContext.RequestAborted
                );
            if (claim is null)
            {
                return CreateVerificationMissingResult(verificationState);
            }

            object? result;
            try
            {
                result = await next(context);
            }
            catch
            {
                await ReleaseVerificationClaimAsync(claim);
                throw;
            }

            if (!IsSuccessfulResult(result))
            {
                await ReleaseVerificationClaimAsync(claim);
            }

            return result;
        }

        private IResult CreateVerificationMissingResult(VerifiedIdVerificationState verificationState)
        {
            return Results.Problem(
                GetVerificationMissingDetail(verificationState),
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?>
                {
                    ["verifiedIdRequired"] = true,
                    ["lastVerifiedAt"] = verificationState.LastVerifiedAt,
                    ["validUntil"] = verificationState.ValidUntil,
                }
            );
        }

        /// <summary>
        /// A verification is accepted while it is within its validity window. When every action requires its own
        /// verification, it must additionally be bound to the action of this endpoint, so a verification started
        /// for one action cannot be used for another.
        /// </summary>
        private bool IsVerificationAcceptedForEndpoint(
            VerifiedIdVerificationState verificationState,
            HttpContext httpContext
        )
        {
            if (!verificationState.IsValid)
            {
                return false;
            }

            if (!_verifiedIdRequirementService.RequiresVerificationPerAction)
            {
                return true;
            }

            VerifiedIdActionMetadata actionMetadata =
                httpContext.GetEndpoint()?.Metadata.GetMetadata<VerifiedIdActionMetadata>()
                ?? throw new InvalidOperationException(
                    $"Endpoints gated by {nameof(RequireRecentVerifiedIdEndpointFilter)} must be registered with {nameof(RequireRecentVerifiedIdExtensions.RequireRecentVerifiedId)}."
                );
            return verificationState.VerifiedForAction == actionMetadata.Action;
        }

        private string GetVerificationMissingDetail(VerifiedIdVerificationState verificationState)
        {
            if (_verifiedIdRequirementService.RequiresVerificationPerAction)
            {
                return Strings.ERROR_VERIFIED_ID_VERIFICATION_REQUIRED_PER_ACTION;
            }

            return verificationState.LastVerifiedAt.HasValue
                ? Strings.ERROR_VERIFIED_ID_VERIFICATION_EXPIRED
                : Strings.ERROR_VERIFIED_ID_VERIFICATION_REQUIRED;
        }

        /// <summary>
        /// Gives the verification back after the action failed, so the user does not have to verify again. The
        /// action result is returned either way, so a failure is logged and the verification stays used.
        /// </summary>
        private async Task ReleaseVerificationClaimAsync(VerifiedIdVerificationClaim claim)
        {
            try
            {
                // Not bound to the request: the action already ran, so the claim is released even if the client
                // disconnected in the meantime.
                await _verifiedIdRequirementService.ReleaseVerificationClaimAsync(
                    claim,
                    CancellationToken.None
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Could not release the Verified ID verification of user {UserId} after the action failed.",
                    claim.UserId
                );
            }
        }

        /// <summary>
        /// <c>true</c> unless the endpoint reported a non-success status code. Results without a status code are
        /// treated as successful, so the verification is rather used up once too often than given back.
        /// </summary>
        private static bool IsSuccessfulResult(object? result)
        {
            return result switch
            {
                INestedHttpResult nestedResult => IsSuccessfulResult(nestedResult.Result),
                IStatusCodeHttpResult { StatusCode: int statusCode } => statusCode
                    is >= StatusCodes.Status200OK
                        and < StatusCodes.Status300MultipleChoices,
                _ => true,
            };
        }
    }
}
