using MyWorkID.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWorkID.Server.UnitTests.TestModels;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class AzureAdConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateAzureAdOptions(TestConfigurationSection testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            var configurationData = testConfiguration.ToKeyValuePairs();
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<AzureAdOptions>(configurationData, AzureAdOptions.SectionName);

            if (expectedExceptionType != null)
            {
                var exception = Assert.Throws(expectedExceptionType, () =>
                {
                    _ = serviceProvider.GetRequiredService<IOptions<AzureAdOptions>>().Value;
                });

                Assert.Contains("DataAnnotation validation failed", exception.Message);
                Assert.Contains(expectedErrorMessage!, exception.Message);
            }
            else
            {
                var options = serviceProvider.GetRequiredService<IOptions<AzureAdOptions>>().Value;
                Assert.NotNull(options);
            }
        }

        public static TheoryData<TestConfigurationSection, Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<TestConfigurationSection, Type?, string?>
            {
                {
                TestConfigurationSection.Create(
                    ("AzureAd:TenantId", Guid.NewGuid().ToString()),
                    ("AzureAd:ClientId", Guid.NewGuid().ToString())
                ),
                typeof(OptionsValidationException),
                "The Instance field is required."
                },
                {
                TestConfigurationSection.Create(
                    ("AzureAd:Instance", "SomeValue"),
                    ("AzureAd:ClientId", Guid.NewGuid().ToString())
                ),
                typeof(OptionsValidationException),
                "The field 'TenantId' must be a valid GUID."
                },
                {
                TestConfigurationSection.Create(
                    ("AzureAd:Instance", "SomeValue"),
                    ("AzureAd:TenantId", "InvalidValue"),
                    ("AzureAd:ClientId", Guid.NewGuid().ToString())
                ),
                typeof(OptionsValidationException),
                "The field 'TenantId' must be a valid GUID."
                },
                {
                TestConfigurationSection.Create(
                    ("AzureAd:Instance", "SomeValue"),
                    ("AzureAd:TenantId", Guid.NewGuid().ToString())
                ),
                typeof(OptionsValidationException),
                "The field 'ClientId' must be a valid GUID."
                },
                {
                TestConfigurationSection.Create(
                    ("AzureAd:Instance", "SomeValue"),
                    ("AzureAd:TenantId", Guid.NewGuid().ToString()),
                    ("AzureAd:ClientId", "InvalidValue")
                ),
                typeof(OptionsValidationException),
                "The field 'ClientId' must be a valid GUID."
                },
                {
                TestConfigurationSection.Create(
                    ("AzureAd:Instance", "SomeValue"),
                    ("AzureAd:TenantId", Guid.NewGuid().ToString()),
                    ("AzureAd:ClientId", Guid.NewGuid().ToString())
                ),
                null,
                null
                }
            };
        }
    }
}
