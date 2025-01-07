using MyWorkID.Server.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public static class ConfigurationTestsHelper
    {
        public static ServiceProvider ConfigureOptions<TOptions>(KeyValuePair<string, string?>[] testConfiguration, string sectionName) where TOptions : BaseOptions, new()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfiguration)
                .Build();

            var services = new ServiceCollection();
            services.AddOptions<TOptions>()
                    .Bind(configuration.GetRequiredSection(sectionName))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
