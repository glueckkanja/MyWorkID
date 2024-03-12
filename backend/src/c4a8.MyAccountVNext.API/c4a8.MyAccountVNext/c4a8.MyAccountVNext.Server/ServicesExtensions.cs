using Azure.Identity;
using c4a8.MyAccountVNext.API.Options;
using c4a8.MyAccountVNext.Server.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;

namespace c4a8.MyAccountVNext.API
{
    public static class ServicesExtensions
    {
        public static void AddGraphClient(this IServiceCollection services, IConfigurationSection graphConfigurationSection)
        {
            ArgumentNullException.ThrowIfNull(graphConfigurationSection);
            services.AddSingleton(new GraphServiceClient(new DefaultAzureCredential()));
        }

        public static void AddConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AppFunctionsOptions>(config.GetSection("AppFunctions"));
            services.Configure<FrontendOptions>(config.GetSection("Frontend"));
        }
    }
}
