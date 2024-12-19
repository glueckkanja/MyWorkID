using c4a8.MyWorkID.Server.Exceptions;
using c4a8.MyWorkID.Server.Features.VerifiedId;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace c4a8.MyWorkID.Server
{
    /// <summary>
    /// Provides extension methods for configuring services.
    /// </summary>
    public static class ServicesExtensions
    {
        /// <summary>
        /// Adds custom authentication services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            VerifiedIdOptions verifiedIdConfig = new();
            configuration.GetSection("VerifiedId").Bind(verifiedIdConfig);
            var signingByte = Encoding.UTF8.GetBytes(verifiedIdConfig.JwtSigningKey ?? throw new ConfigurationMissingException(nameof(verifiedIdConfig.JwtSigningKey), "JwtSigningKey is missing in configuration."));

            // Add services to the container.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(Strings.VERIFIED_ID_CALLBACK_SCHEMA, options =>
                {
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(signingByte);
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidateAudience = false;
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                })
                .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

            services.AddAuthorizationBuilder()
               .AddPolicy(Strings.VERIFIED_ID_CALLBACK_POLICY, policy =>
               {
                   policy.RequireAuthenticatedUser();
                   policy.AuthenticationSchemes.Add(Strings.VERIFIED_ID_CALLBACK_SCHEMA);
               });
        }
    }
}