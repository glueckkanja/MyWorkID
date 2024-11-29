using Azure.Core;
using Azure.Identity;
using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.VerifiedId.HttpClients;
using c4a8.MyWorkID.Server.Features.VerifiedId.SignalR;

namespace c4a8.MyWorkID.Server.Features.VerifiedId
{
    public class VerifiedIdModule : IModule
    {
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
            services.AddSingleton<IVerifiedIdSignalRRepository, VerifiedIdSignalRRepository>();
        }
    }
}
