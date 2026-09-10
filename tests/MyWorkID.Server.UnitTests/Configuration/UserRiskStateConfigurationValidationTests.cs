using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWorkID.Server.Options;
using MyWorkID.Server.UnitTests.TestModels;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class UserRiskStateConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateUserRiskStateConfigOptions(TestConfigurationSection testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            var configurationData = testConfiguration.ToKeyValuePairs();
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<UserRiskStateOptions>(configurationData, UserRiskStateOptions.SectionName, sectionRequired: false);

            if (expectedExceptionType != null)
            {
                var exception = Assert.Throws(expectedExceptionType, () =>
                {
                    _ = serviceProvider.GetRequiredService<IOptions<UserRiskStateOptions>>().Value;
                });

                Assert.Contains("DataAnnotation validation failed", exception.Message);
                Assert.Contains(expectedErrorMessage!, exception.Message);
            }
            else
            {
                var options = serviceProvider.GetRequiredService<IOptions<UserRiskStateOptions>>().Value;
                Assert.NotNull(options);
            }
        }

        public static TheoryData<TestConfigurationSection, Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<TestConfigurationSection, Type?, string?>
            {
                // Empty section: default (Low) applies
                {
                    TestConfigurationSection.Create(),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("UserRiskState:MaxDismissibleRiskLevel", "Low")
                    ),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("UserRiskState:MaxDismissibleRiskLevel", "Medium")
                    ),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("UserRiskState:MaxDismissibleRiskLevel", "High")
                    ),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("UserRiskState:MaxDismissibleRiskLevel", "None")
                    ),
                    typeof(OptionsValidationException),
                    "MaxDismissibleRiskLevel must be one of: Low, Medium, High."
                },
                {
                    TestConfigurationSection.Create(
                        ("UserRiskState:MaxDismissibleRiskLevel", "Hidden")
                    ),
                    typeof(OptionsValidationException),
                    "MaxDismissibleRiskLevel must be one of: Low, Medium, High."
                }
            };
        }
    }
}
