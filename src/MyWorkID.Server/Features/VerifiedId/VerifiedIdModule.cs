using Azure.Core;
using Azure.Identity;
using MyWorkID.Server.Common;
using MyWorkID.Server.Features.VerifiedId.HttpClients;
using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Module for configuring services related to Verified ID operations.
    /// </summary>
    public class VerifiedIdModule : IModule
    {
        /// <summary>
        /// Configures the services required for Verified ID operations.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configurationManager">The configuration manager to retrieve settings from.</param>
        /// <param name="environment">The web host environment.</param>
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment)
        {
            services.Configure<VerifiedIdOptions>(configurationManager.GetSection("VerifiedId"));
            TokenCredential verifiedIdCredentials = new ManagedIdentityCredential();
            if (environment.IsDevelopment())
            {
                verifiedIdCredentials = new ClientSecretCredential(
                    configurationManager.GetValue<string>("LocalDevSettings:VerifiedIdTenantId"),
                    configurationManager.GetValue<string>("LocalDevSettings:VerifiedIdClientId"),
                    configurationManager.GetValue<string>("LocalDevSettings:VerifiedIdSecret"));
            }
            services.AddTransient<VerifiedIdAuthenticationHandler>();
            services.AddSingleton(new VerifiedIdAccessTokenService(verifiedIdCredentials));
            services.AddHttpClient<VerifiedIdService>().AddHttpMessageHandler<VerifiedIdAuthenticationHandler>();
            services.AddScoped<IVerifiedIdService, VerifiedIdService>();
            services.AddSingleton<IVerifiedIdSignalRRepository, VerifiedIdSignalRRepository>();
        }
    }
}
