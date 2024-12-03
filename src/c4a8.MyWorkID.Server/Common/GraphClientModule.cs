﻿using Azure.Identity;
using Microsoft.Graph;

namespace c4a8.MyWorkID.Server.Common
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
            // Ensure the "MsGraph" section is present in the configuration
            ArgumentNullException.ThrowIfNull(configurationManager.GetSection("MsGraph"));

            // Add the GraphServiceClient to the service collection with token credentials
            services.AddSingleton(new GraphServiceClient(new ChainedTokenCredential(new ManagedIdentityCredential(), new DefaultAzureCredential())));
        }
    }
}
