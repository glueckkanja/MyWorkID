using c4a8.MyAccountVNext.API.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace c4a8.MyAccountVNext.API.Services
{
    public class AppSettingsAuthContextService : IAuthContextService
    {
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public AppSettingsAuthContextService(IOptions<AppFunctionsOptions> appFunctionsOptions)
        {
            _appFunctionsOptions = appFunctionsOptions.Value;
        }

        public async Task AddClaimsChallengeHeader(HttpContext httpContext, string authContextId)
        {
            var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authContextId + "\"}}}"));
            httpContext.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", error=\"insufficient_claims\", claims=\"" + base64str + "\"");
            httpContext.Response.Headers.Append("Access-Control-Expose-Headers", "WWW-Authenticate");
            string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
            await httpContext.Response.WriteAsync(message);
            await httpContext.Response.CompleteAsync();
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
