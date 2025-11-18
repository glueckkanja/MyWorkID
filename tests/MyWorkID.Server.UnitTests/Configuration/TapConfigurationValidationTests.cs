using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class TapConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateTapConfigOptions(KeyValuePair<string, string?>[] testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<TapOptions>(testConfiguration, TapOptions.SectionName, sectionRequired: false);

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

        public static TheoryData<KeyValuePair<string, string?>[], Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<KeyValuePair<string, string?>[], Type?, string?>
            {
                {
                    new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Tap:LifetimeInMinutes", "9")
                    },
                    typeof(OptionsValidationException),
                    "The field 'LifetimeInMinutes' must be between 10 and 43200."
                },
                {
                    new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Tap:LifetimeInMinutes", "43201")
                    },
                    typeof(OptionsValidationException),
                    "The field 'LifetimeInMinutes' must be between 10 and 43200."
                },
                {
                    Array.Empty<KeyValuePair<string, string?>>(),
                    null,
                    null
                },
                {
                    new KeyValuePair<string, string?>[]
                    {
                        new KeyValuePair<string, string?>("Tap:LifetimeInMinutes", "30"),
                        new KeyValuePair<string, string?>("Tap:IsUsableOnce", "true")
                    },
                    null,
                    null
                }
            };
        }
    }
}
