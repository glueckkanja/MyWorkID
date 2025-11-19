using MyWorkID.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWorkID.Server.UnitTests.TestModels;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class FrontendConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateFrontendConfigOptions(TestConfigurationSection testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            var configurationData = testConfiguration.ToKeyValuePairs();
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<FrontendOptions>(configurationData, FrontendOptions.SectionName);

            if (expectedExceptionType != null)
            {
                var exception = Assert.Throws(expectedExceptionType, () =>
                {
                    _ = serviceProvider.GetRequiredService<IOptions<FrontendOptions>>().Value;
                });

                Assert.Contains("DataAnnotation validation failed", exception.Message);
                Assert.Contains(expectedErrorMessage!, exception.Message);
            }
            else
            {
                var options = serviceProvider.GetRequiredService<IOptions<FrontendOptions>>().Value;
                Assert.NotNull(options);
            }
        }

        public static TheoryData<TestConfigurationSection, Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<TestConfigurationSection, Type?, string?>
            {
                {
                    TestConfigurationSection.Create(
                        ("Frontend:TenantId", Guid.NewGuid().ToString()),
                        ("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    ),
                            typeof(OptionsValidationException),
                            "The field 'FrontendClientId' must be a valid GUID."
                        },
                        {
                            TestConfigurationSection.Create(
                        ("Frontend:FrontendClientId", "InvalidValue"),
                        ("Frontend:TenantId", Guid.NewGuid().ToString()),
                        ("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    ),
                            typeof(OptionsValidationException),
                            "The field 'FrontendClientId' must be a valid GUID."
                        },
                        {
                            TestConfigurationSection.Create(
                        ("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        ("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    ),
                            typeof(OptionsValidationException),
                            "The field 'TenantId' must be a valid GUID."
                        },
                        {
                            TestConfigurationSection.Create(
                        ("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        ("Frontend:TenantId", "InvalidValue"),
                        ("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    ),
                            typeof(OptionsValidationException),
                            "The field 'TenantId' must be a valid GUID."
                        },
                        {
                            TestConfigurationSection.Create(
                        ("Frontend:TenantId", Guid.NewGuid().ToString()),
                        ("Frontend:FrontendClientId", Guid.NewGuid().ToString())
                    ),
                            typeof(OptionsValidationException),
                            "The field 'BackendClientId' must be a valid GUID."
                        },
                        {
                            TestConfigurationSection.Create(
                        ("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        ("Frontend:TenantId", Guid.NewGuid().ToString()),
                        ("Frontend:BackendClientId", "InvalidValue")
                    ),
                            typeof(OptionsValidationException),
                            "The field 'BackendClientId' must be a valid GUID."
                        },
                        {
                            TestConfigurationSection.Create(
                        ("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        ("Frontend:TenantId", Guid.NewGuid().ToString()),
                        ("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    ),
                    null,
                    null
                }
            };
        }
    }
}
