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

            if (!verificationState.IsValid)
            {
                string detail = verificationState.LastVerifiedAt.HasValue
                    ? Strings.ERROR_VERIFIED_ID_VERIFICATION_EXPIRED
                    : Strings.ERROR_VERIFIED_ID_VERIFICATION_REQUIRED;
                return Results.Problem(
                    detail,
                    statusCode: StatusCodes.Status403Forbidden,
                    extensions: new Dictionary<string, object?>
                    {
                        ["verifiedIdRequired"] = true,
                        ["lastVerifiedAt"] = verificationState.LastVerifiedAt,
                        ["validUntil"] = verificationState.ValidUntil,
                    }
                );
            }

            return await next(context);
        }
    }
}
