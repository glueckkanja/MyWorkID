using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWorkID.Server.Options;
using MyWorkID.Server.UnitTests.TestModels;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class TapConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateTapConfigOptions(TestConfigurationSection testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            var configurationData = testConfiguration.ToKeyValuePairs();
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<TapOptions>(configurationData, TapOptions.SectionName, sectionRequired: false);

            if (expectedExceptionType != null)
            {
                var exception = Assert.Throws(expectedExceptionType, () =>
                {
                    _ = serviceProvider.GetRequiredService<IOptions<TapOptions>>().Value;
                });

                Assert.Contains("DataAnnotation validation failed", exception.Message);
                Assert.Contains(expectedErrorMessage!, exception.Message);
            }
            else
            {
                var options = serviceProvider.GetRequiredService<IOptions<TapOptions>>().Value;
                Assert.NotNull(options);
            }
        }

        public static TheoryData<TestConfigurationSection, Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<TestConfigurationSection, Type?, string?>
            {
                {
                    TestConfigurationSection.Create(
                        ("Tap:LifetimeInMinutes", "9")
                    ),
                    typeof(OptionsValidationException),
                    "The field 'LifetimeInMinutes' must be between 10 and 43200."
                },
                {
                    TestConfigurationSection.Create(
                        ("Tap:LifetimeInMinutes", "43201")
                    ),
                    typeof(OptionsValidationException),
                    "The field 'LifetimeInMinutes' must be between 10 and 43200."
                },
                {
                    TestConfigurationSection.Create(),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("Tap:LifetimeInMinutes", "30"),
                        ("Tap:IsUsableOnce", "true")
                    ),
                    null,
                    null
                }
            };
        }
    }
}
