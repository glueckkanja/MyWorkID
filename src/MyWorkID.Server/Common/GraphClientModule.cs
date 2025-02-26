using Azure.Identity;
using Microsoft.Graph;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Common
{
    /// <summary>
    /// Module for configuring the Microsoft Graph client services.
    /// </summary>
    public class GraphClientModule : IModule
    {
        /// <summary>
        /// Configures the services required for the Microsoft Graph client.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="environment">The web host environment.</param>
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment)
        {
            IdentityOptions? identity = configurationManager.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>();
            services.AddSingleton(new GraphServiceClient(new ChainedTokenCredential(new ManagedIdentityCredential(clientId: identity?.ManagedIdentityClientId), new DefaultAzureCredential())));
        }
    }
}
