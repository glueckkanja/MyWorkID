using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace c4a8.MyWorkID.Server.Common
{
    /// <summary>
    /// Service for handling authentication context.
    /// </summary>
    public class AuthContextService : IAuthContextService, IModule
    {
        private readonly AppFunctionsOptions _appFunctionsOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthContextService"/> class.
        /// </summary>
        /// <param name="appFunctionsOptions">The app functions options.</param>
        public AuthContextService(IOptions<AppFunctionsOptions> appFunctionsOptions)
        {
            _appFunctionsOptions = appFunctionsOptions.Value;
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="environment">The web host environment.</param>
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment)
        {
            services.Configure<AppFunctionsOptions>(configurationManager.GetSection("AppFunctions"));
            services.AddScoped<IAuthContextService, AuthContextService>();
        }

        /// <summary>
        /// Adds headers to the response to challenge the client to provide the required claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="authContextId">Id of authentication context required for authorization.</param>
        public void AddClaimsChallengeHeader(HttpContext httpContext, string authContextId)
        {
            var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authContextId + "\"}}}"));
            httpContext.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", error=\"insufficient_claims\", claims=\"" + base64str + "\"");
            httpContext.Response.Headers.Append("Access-Control-Expose-Headers", "WWW-Authenticate");
        }

        /// <summary>
        /// Checks if the required authentication context is present in the claims.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="appFunction">Function/feature of the app.</param>
        /// <returns>Claims challenge to be returned to the user to authenticate with correct authentication context. If claim is present, no claims challenge is returned.</returns>
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

        /// <summary>
        /// Gets the authentication context ID for a given app function.
        /// </summary>
        /// <param name="appFunction">The app function.</param>
        /// <returns>The authentication context ID.</returns>
        public string? GetAuthContextId(AppFunctions appFunction)
        {
            return appFunction switch
            {
                AppFunctions.DismissUserRisk => _appFunctionsOptions.DismissUserRisk,
                AppFunctions.GenerateTap => _appFunctionsOptions.GenerateTap,
                AppFunctions.ResetPassword => _appFunctionsOptions.ResetPassword,
                _ => null,
            };
        }

        /// <summary>
        /// Gets the claims challenge message.
        /// </summary>
        /// <returns>The claims challenge message.</returns>
        public string GetClaimsChallengeMessage()
        {
            return Strings.ERROR_INSUFFICIENT_CLAIMS;
        }
    }
}
