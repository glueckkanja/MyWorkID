namespace MyWorkID.Server.Common
{
    /// <summary>
    /// Defines a contract for configuring services in the application.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Configures the services required by the module.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="environment">The web host environment.</param>
        static abstract void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment);
    }
}
