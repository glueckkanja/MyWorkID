using Azure.Identity;
using c4a8.MyAccountVNext.API.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;

namespace c4a8.MyAccountVNext.API
{
    public static class ServicesExtensions
    {
        public static void AddGraphClient(this IServiceCollection services, IConfigurationSection graphConfigurationSection)
        {
            ArgumentNullException.ThrowIfNull(graphConfigurationSection);

            MsGraphOptions msGraphOptions = new();
            graphConfigurationSection.Bind(msGraphOptions);
            services.AddSingleton(new GraphServiceClient(new DefaultAzureCredential()));
        }

        public static void AddConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MsGraphOptions>(config.GetSection("MsGraph"));
            services.Configure<AppFunctionsOptions>(config.GetSection("AppFunctions"));
        }
    }
}
