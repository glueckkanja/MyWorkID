using MyWorkID.Server.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public static class ConfigurationTestsHelper
    {
        public static ServiceProvider ConfigureOptions<TOptions>(KeyValuePair<string, string?>[] testConfiguration, string sectionName, bool sectionRequired = true) where TOptions : BaseOptions, new()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfiguration)
                .Build();

            var services = new ServiceCollection();
            var section = sectionRequired
                ? configuration.GetRequiredSection(sectionName)
                : configuration.GetSection(sectionName);
            services.AddOptions<TOptions>()
                    .Bind(section)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
