using c4a8.MyWorkID.Server.Common;

namespace c4a8.MyWorkID.Server.Features.Configuration
{
    /// <summary>
    /// Configures services related to the application's configuration.
    /// </summary>
    public class ConfigurationModule : IModule
    {
        /// <summary>
        /// Configures the services required for the configuration module.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configurationManager">The configuration manager to retrieve configuration settings from.</param>
        /// <param name="environment">The web host environment.</param>
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment)
        {
            // Configures FrontendOptions using the "Frontend" section from the configuration
            services.Configure<FrontendOptions>(configurationManager.GetSection("Frontend"));
        }
    }
}
