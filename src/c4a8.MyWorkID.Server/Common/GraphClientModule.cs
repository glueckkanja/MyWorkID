using Azure.Identity;
using Microsoft.Graph;

namespace c4a8.MyWorkID.Server.Common
{
    public class GraphClientModule : IModule
    {
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment Environment)
        {
            ArgumentNullException.ThrowIfNull(configurationManager.GetSection("MsGraph"));
            services.AddSingleton(new GraphServiceClient(new ChainedTokenCredential(new ManagedIdentityCredential(), new DefaultAzureCredential())));
        }
    }
}
