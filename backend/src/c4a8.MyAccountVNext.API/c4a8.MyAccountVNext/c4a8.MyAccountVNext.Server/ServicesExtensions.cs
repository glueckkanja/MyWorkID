using c4a8.MyAccountVNext.Server;
using c4a8.MyAccountVNext.Server.Features.VerifiedId;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace c4a8.MyAccountVNext.API
{
    public static class ServicesExtensions
    {
        public static void AddVerifiedIdAuthentication(this IServiceCollection services, IConfiguration configuration,
            string authenticationSchemaName)
        {
            VerifiedIdOptions verifiedIdConfig = new();
            configuration.GetSection("VerifiedId").Bind(verifiedIdConfig);
            var signingByte = Encoding.UTF8.GetBytes(verifiedIdConfig.JwtSigningKey ?? "GodDamnitYouForgottToSpecifyASigningKey");

            // Add services to the container.
            services.AddAuthentication(authenticationSchemaName)
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
        }
    }
}