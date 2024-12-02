using c4a8.MyWorkID.Server.Common;

namespace c4a8.MyWorkID.Server.Features.Configuration
{
    public class ConfigurationModule : IModule
    {
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment environment)
        {
            services.Configure<FrontendOptions>(configurationManager.GetSection("Frontend"));
        }
    }
}
