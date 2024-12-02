using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace c4a8.MyWorkID.Server.Common
{
    public class AuthContextService : IAuthContextService, IModule
    {
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public AuthContextService(IOptions<AppFunctionsOptions> appFunctionsOptions)
        {
            _appFunctionsOptions = appFunctionsOptions.Value;
        }

        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment)
        {
            services.Configure<AppFunctionsOptions>(configurationManager.GetSection("AppFunctions"));
            services.AddScoped<IAuthContextService, AuthContextService>();
        }

        public void AddClaimsChallengeHeader(HttpContext httpContext, string authContextId)
        {
            var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authContextId + "\"}}}"));
            httpContext.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", error=\"insufficient_claims\", claims=\"" + base64str + "\"");
            httpContext.Response.Headers.Append("Access-Control-Expose-Headers", "WWW-Authenticate");
        }

        public string CheckForRequiredAuthContext(HttpContext context, AppFunctions appFunction)
        {
            string claimsChallenge = string.Empty;
            string? authContextId = GetAuthContextId(appFunction);

            if (!string.IsNullOrEmpty(authContextId))
            {
                Claim? acrsClaim = context.User.FindAll("acrs").FirstOrDefault(x => x.Value == authContextId);

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

        public string GetClaimsChallengeMessage()
        {
            return Strings.ERROR_INSUFFICIENT_CLAIMS;
        }
    }
}
