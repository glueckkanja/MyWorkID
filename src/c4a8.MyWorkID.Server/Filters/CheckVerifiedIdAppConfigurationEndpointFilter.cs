using c4a8.MyWorkID.Server.Options;
using Microsoft.Extensions.Options;

namespace c4a8.MyWorkID.Server.Filters
{
    /// <summary>
    /// Endpoint filter to check if app is configured with for VerifiedId function.
    /// </summary>
    class CheckVerifiedIdAppConfigurationEndpointFilter : IEndpointFilter
    {
        private readonly VerifiedIdOptions _verifiedIdOptions;

        public CheckVerifiedIdAppConfigurationEndpointFilter(IOptions<VerifiedIdOptions> verifiedIdOptions)
        {
            _verifiedIdOptions = verifiedIdOptions.Value;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (_verifiedIdOptions is null ||
                string.IsNullOrWhiteSpace(_verifiedIdOptions.BackendUrl) ||
                string.IsNullOrWhiteSpace(_verifiedIdOptions.CreatePresentationRequestUri) ||
                string.IsNullOrWhiteSpace(_verifiedIdOptions.DecentralizedIdentifier) ||
                string.IsNullOrWhiteSpace(_verifiedIdOptions.JwtSigningKey) ||
                string.IsNullOrWhiteSpace(_verifiedIdOptions.TargetSecurityAttribute) ||
                string.IsNullOrWhiteSpace(_verifiedIdOptions.TargetSecurityAttributeSet))
            {
                return Results.Problem(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID, statusCode: StatusCodes.Status500InternalServerError);
            }

            return await next(context);
        }
    }
}
