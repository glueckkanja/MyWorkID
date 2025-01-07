using MyWorkID.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class FrontendConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateFrontendConfigOptions(KeyValuePair<string, string?>[] testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<FrontendOptions>(testConfiguration, FrontendOptions.SectionName);

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

        public static TheoryData<KeyValuePair<string, string?>[], Type, string> GetTestConfigurations()
        {
            return new TheoryData<KeyValuePair<string, string?>[], Type, string>
            {
                {
                    new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:TenantId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    },
                            typeof(OptionsValidationException),
                            "The field 'FrontendClientId' must be a valid GUID."
                        },
                        {
                            new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:FrontendClientId", "InvalidValue"),
                        new KeyValuePair<string, string?>("Frontend:TenantId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    },
                            typeof(OptionsValidationException),
                            "The field 'FrontendClientId' must be a valid GUID."
                        },
                        {
                            new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    },
                            typeof(OptionsValidationException),
                            "The field 'TenantId' must be a valid GUID."
                        },
                        {
                            new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:TenantId", "InvalidValue"),
                        new KeyValuePair<string, string?>("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    },
                            typeof(OptionsValidationException),
                            "The field 'TenantId' must be a valid GUID."
                        },
                        {
                            new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:TenantId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:FrontendClientId", Guid.NewGuid().ToString())
                    },
                            typeof(OptionsValidationException),
                            "The field 'BackendClientId' must be a valid GUID."
                        },
                        {
                            new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:TenantId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:BackendClientId", "InvalidValue")
                    },
                            typeof(OptionsValidationException),
                            "The field 'BackendClientId' must be a valid GUID."
                        },
                        {
                            new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Frontend:FrontendClientId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:TenantId", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string?>("Frontend:BackendClientId", Guid.NewGuid().ToString())
                    },
                    null,
                    null
                }
            };
        }
    }
}
