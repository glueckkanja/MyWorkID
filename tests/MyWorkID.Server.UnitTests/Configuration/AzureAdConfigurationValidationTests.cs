using MyWorkID.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class AzureAdConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateAzureAdOptions(KeyValuePair<string, string?>[] testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<AzureAdOptions>(testConfiguration, AzureAdOptions.SectionName);

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

        public static TheoryData<KeyValuePair<string, string?>[], Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<KeyValuePair<string, string?>[], Type?, string?>
            {
                {
                new KeyValuePair<string, string?>[]
                {
                    new KeyValuePair<string, string?>("AzureAd:TenantId", Guid.NewGuid().ToString()),
                    new KeyValuePair<string, string?>("AzureAd:ClientId", Guid.NewGuid().ToString())
                },
                typeof(OptionsValidationException),
                "The Instance field is required."
                },
                {
                new KeyValuePair<string, string?>[]
                {
                    new KeyValuePair<string, string?>("AzureAd:Instance", "SomeValue"),
                    new KeyValuePair<string, string?>("AzureAd:ClientId", Guid.NewGuid().ToString())
                },
                typeof(OptionsValidationException),
                "The field 'TenantId' must be a valid GUID."
                },
                {
                new KeyValuePair<string, string?>[]
                {
                    new KeyValuePair<string, string?>("AzureAd:Instance", "SomeValue"),
                    new KeyValuePair<string, string?>("AzureAd:TenantId", "InvalidValue"),
                    new KeyValuePair<string, string?>("AzureAd:ClientId", Guid.NewGuid().ToString())
                },
                typeof(OptionsValidationException),
                "The field 'TenantId' must be a valid GUID."
                },
                {
                new KeyValuePair<string, string?>[]
                {
                    new KeyValuePair<string, string?>("AzureAd:Instance", "SomeValue"),
                    new KeyValuePair<string, string?>("AzureAd:TenantId", Guid.NewGuid().ToString())
                },
                typeof(OptionsValidationException),
                "The field 'ClientId' must be a valid GUID."
                },
                {
                new KeyValuePair<string, string?>[]
                {
                    new KeyValuePair<string, string?>("AzureAd:Instance", "SomeValue"),
                    new KeyValuePair<string, string?>("AzureAd:TenantId", Guid.NewGuid().ToString()),
                    new KeyValuePair<string, string?>("AzureAd:ClientId", "InvalidValue")
                },
                typeof(OptionsValidationException),
                "The field 'ClientId' must be a valid GUID."
                },
                {
                new KeyValuePair<string, string?>[]
                {
                    new KeyValuePair<string, string?>("AzureAd:Instance", "SomeValue"),
                    new KeyValuePair<string, string?>("AzureAd:TenantId", Guid.NewGuid().ToString()),
                    new KeyValuePair<string, string?>("AzureAd:ClientId", Guid.NewGuid().ToString())
                },
                null,
                null
                }
            };
        }
    }
}
