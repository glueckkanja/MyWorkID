using Azure.Identity;
using c4a8.MyAccountVNext.Server.HttpClients.MsGraph;
using Microsoft.Graph;

namespace c4a8.MyAccountVNext.Server.Common
{
    public class GraphClientModule : IModule
    {
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment Environment)
        {
            ArgumentNullException.ThrowIfNull(configurationManager.GetSection("MsGraph"));
            var loggerFactory = LoggerFactory.Create(x => x.AddConsole());
            var logger = loggerFactory.CreateLogger<LogGraphRequestHandler>();
            var handlers = GraphClientFactory.CreateDefaultHandlers();
            handlers.Add(new LogGraphRequestHandler(logger));
            var httpClient = GraphClientFactory.Create(handlers);

            //services.AddSingleton(new GraphServiceClient(httpClient, new ClientSecretCredential("827c972c-16ec-48a4-9ff2-76bd30177a5b", "1657d55a-8c46-4ea9-9ce3-313b71ffecbf", "vpo8Q~eu73Uf0NLZ1_~oy9V2naqix1_vqtqUycgy")));
            services.AddSingleton(new GraphServiceClient(httpClient, new ChainedTokenCredential(new ManagedIdentityCredential(), new DefaultAzureCredential())));
            //services.AddSingleton(new GraphServiceClient(new ChainedTokenCredential(new ManagedIdentityCredential(), new DefaultAzureCredential())));
        }
    }
}
