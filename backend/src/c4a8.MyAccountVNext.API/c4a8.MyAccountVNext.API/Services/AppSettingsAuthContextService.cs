using c4a8.MyAccountVNext.API.Options;
using Microsoft.Extensions.Options;

namespace c4a8.MyAccountVNext.API.Services
{
    public class AppSettingsAuthContextService : IAuthContextService
    {
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public AppSettingsAuthContextService(IOptions<AppFunctionsOptions> appFunctionsOptions)
        {
            _appFunctionsOptions = appFunctionsOptions.Value;
        }

        public string? GetAuthContextId(AppFunctions appFunction)
        {
            switch (appFunction)
            {
                case AppFunctions.DismissUserRisk:
                {
                    return _appFunctionsOptions.DismissUserRisk;
                    }
                case AppFunctions.GenerateTap:
                    {
                        return _appFunctionsOptions.GenerateTap;
                    }
                case AppFunctions.ResetPassword:
                    {
                        return _appFunctionsOptions.ResetPassword;
                    }
                default: return null;
            }
        }
    }
}
