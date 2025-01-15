using MyWorkID.Server.Options;
using MyWorkID.Server.Validation;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Filters
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
            IList<ValidationResult> results = DataAnnotationsValidator.Validate(_verifiedIdOptions);
            if (results.Count > 0)
            {
                var errors = string.Join(" ", results.Select(x => x.ErrorMessage));
                return Results.Problem($"{Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID} - {errors}", statusCode: StatusCodes.Status500InternalServerError);
            }

            return await next(context);
        }
    }
}
