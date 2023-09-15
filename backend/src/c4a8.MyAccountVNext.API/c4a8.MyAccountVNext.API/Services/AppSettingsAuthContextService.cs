using c4a8.MyAccountVNext.API.Options;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.API.Services
{
    public class AppSettingsAuthContextService : IAuthContextService
    {
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public AppSettingsAuthContextService(IOptions<AppFunctionsOptions> appFunctionsOptions)
        {
            _appFunctionsOptions = appFunctionsOptions.Value;
        }

        public string CheckForRequiredAuthContext(HttpContext context, AppFunctions appFunction)
        {
            string claimsChallenge = string.Empty;
            string? authContextId = GetAuthContextId(appFunction);

            if (!string.IsNullOrEmpty(authContextId))
            {
                string authenticationContextClassReferencesClaim = "acrs";

                if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
                {
                    throw new ArgumentNullException("No Usercontext is available to pick claims from");
                }

                Claim acrsClaim = context.User.FindAll(authenticationContextClassReferencesClaim).FirstOrDefault(x => x.Value == authContextId);

                if (acrsClaim?.Value != authContextId)
                {
                    claimsChallenge = "{\"id_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authContextId + "\"}}}";

                }
            }

            return claimsChallenge;
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
