using Azure.Core;
using Azure.Identity;
using c4a8.MyAccountVNext.API.Options;
using c4a8.MyAccountVNext.Server.HttpClients;
using c4a8.MyAccountVNext.Server.HttpClients.MsGraph;
using c4a8.MyAccountVNext.Server.HttpClients.VerifiedId;
using c4a8.MyAccountVNext.Server.Options;
using Microsoft.Graph;

namespace c4a8.MyAccountVNext.API
{
    public static class ServicesExtensions
    {
        public static void AddGraphClient(this IServiceCollection services, IConfigurationSection graphConfigurationSection)
        {
            ArgumentNullException.ThrowIfNull(graphConfigurationSection);
            var loggerFactory = LoggerFactory.Create(x => x.AddAzureWebAppDiagnostics());
            var logger = loggerFactory.CreateLogger<LogGraphRequestHandler>();
            var handlers = GraphClientFactory.CreateDefaultHandlers();
            handlers.Add(new LogGraphRequestHandler(logger));
            var httpClient = GraphClientFactory.Create(handlers);
            services.AddSingleton(new GraphServiceClient(httpClient, new ChainedTokenCredential(new ManagedIdentityCredential(), new DefaultAzureCredential())));
        }

        public static void AddConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AppFunctionsOptions>(config.GetSection("AppFunctions"));
            services.Configure<FrontendOptions>(config.GetSection("Frontend"));
            services.Configure<VerifiedIdOptions>(config.GetSection("VerifiedId"));
        }

        public static void AddVerifiedIdHttpClient<TInjectionTarget>(this IServiceCollection services, TokenCredential verifiedIdTokenCredentials) where TInjectionTarget : class
        {
            services.AddTransient<VerifiedIdAuthenticationHandler>();
            services.AddSingleton(new VerifiedIdAccessTokenService(verifiedIdTokenCredentials));
            services.AddHttpClient<TInjectionTarget>().AddHttpMessageHandler<VerifiedIdAuthenticationHandler>();
        }
    }
}
