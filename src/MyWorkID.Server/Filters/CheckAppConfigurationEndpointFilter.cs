using MyWorkID.Server.Common;
using MyWorkID.Server.Options;
using MyWorkID.Server.Validation;
using Microsoft.Extensions.Options;

namespace MyWorkID.Server.Filters
{
    public class CheckAppConfigurationEndpointFilter : IEndpointFilter
    {
        private readonly AppFunctions _appFunction;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public CheckAppConfigurationEndpointFilter(AppFunctions appFunction, IOptions<AppFunctionsOptions> appFunctionsOptions)
        {
            _appFunction = appFunction;
            _appFunctionsOptions = appFunctionsOptions.Value;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            switch (_appFunction)
            {
                case AppFunctions.DismissUserRisk:
                    if (string.IsNullOrEmpty(_appFunctionsOptions.DismissUserRisk) || !AuthContextValidator.IsValidAuthContext(_appFunctionsOptions.DismissUserRisk))
                    {
                        return Results.Problem(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_DISMISS_USER_RISK, statusCode: StatusCodes.Status500InternalServerError);
                    }
                    break;

                case AppFunctions.GenerateTap:
                    if (string.IsNullOrEmpty(_appFunctionsOptions.GenerateTap) || !AuthContextValidator.IsValidAuthContext(_appFunctionsOptions.GenerateTap))
                    {
                        return Results.Problem(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_GENERATE_TAP, statusCode: StatusCodes.Status500InternalServerError);
                    }
                    break;

                case AppFunctions.ResetPassword:
                    if (string.IsNullOrEmpty(_appFunctionsOptions.ResetPassword) || !AuthContextValidator.IsValidAuthContext(_appFunctionsOptions.ResetPassword))
                    {
                        return Results.Problem(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_RESET_PASSWORD, statusCode: StatusCodes.Status500InternalServerError);
                    }
                    break;

                default:
                    return Results.Problem("Unknown application function", statusCode: StatusCodes.Status500InternalServerError);
            }

            return await next(context);
        }
    }

    /// <summary>
    /// Endpoint filter to check if app is configured with auth context for DismissUserRisk function.
    /// </summary>
    class CheckGenerateTapAppConfigurationEndpointFilter : CheckAppConfigurationEndpointFilter
    {
        public CheckGenerateTapAppConfigurationEndpointFilter(IOptions<AppFunctionsOptions> appFunctionsOptions) : base(AppFunctions.GenerateTap, appFunctionsOptions) { }
    }

    /// <summary>
    /// Endpoint filter to check if app is configured with auth context for ResetPassword function.
    /// </summary>
    class CheckResetPasswordAppConfigurationEndpointFilter : CheckAppConfigurationEndpointFilter
    {
        public CheckResetPasswordAppConfigurationEndpointFilter(IOptions<AppFunctionsOptions> appFunctionsOptions) : base(AppFunctions.ResetPassword, appFunctionsOptions) { }
    }

    /// <summary>
    /// Endpoint filter to check if app is configured with auth context for DismissUserRisk function.
    /// </summary>
    class CheckDismissUserRiskAppConfigurationEndpointFilter : CheckAppConfigurationEndpointFilter
    {
        public CheckDismissUserRiskAppConfigurationEndpointFilter(IOptions<AppFunctionsOptions> appFunctionsOptions) : base(AppFunctions.DismissUserRisk, appFunctionsOptions) { }
    }
}
