using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyWorkID.Server.Options;
using MyWorkID.Server.UnitTests.TestModels;

namespace MyWorkID.Server.UnitTests.Configuration
{
    public class VerifiedIdRequirementConfigurationValidationTests
    {
        [Theory]
        [MemberData(nameof(GetTestConfigurations))]
        public void ValidateVerifiedIdRequirementConfigOptions(TestConfigurationSection testConfiguration, Type? expectedExceptionType, string? expectedErrorMessage)
        {
            var configurationData = testConfiguration.ToKeyValuePairs();
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<VerifiedIdRequirementOptions>(configurationData, VerifiedIdRequirementOptions.SectionName, sectionRequired: false);

            if (expectedExceptionType != null)
            {
                var exception = Assert.Throws(expectedExceptionType, () =>
                {
                    _ = serviceProvider.GetRequiredService<IOptions<VerifiedIdRequirementOptions>>().Value;
                });

                Assert.Contains("DataAnnotation validation failed", exception.Message);
                Assert.Contains(expectedErrorMessage!, exception.Message);
            }
            else
            {
                var options = serviceProvider.GetRequiredService<IOptions<VerifiedIdRequirementOptions>>().Value;
                Assert.NotNull(options);
            }
        }

        [Fact]
        public void EmptySection_IsDisabledWithDefaultValidity()
        {
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<VerifiedIdRequirementOptions>([], VerifiedIdRequirementOptions.SectionName, sectionRequired: false);

            var options = serviceProvider.GetRequiredService<IOptions<VerifiedIdRequirementOptions>>().Value;

            Assert.False(options.Enabled);
            Assert.Equal(VerifiedIdRequirementOptions.DEFAULT_VALIDITY_IN_MINUTES, options.ValidityInMinutes);
            Assert.Equal(TimeSpan.FromHours(24), options.Validity);
        }

        [Fact]
        public void ConfiguredSection_BindsEnabledAndValidity()
        {
            var configurationData = TestConfigurationSection.Create(
                ("VerifiedIdRequirement:Enabled", "true"),
                ("VerifiedIdRequirement:ValidityInMinutes", "90")
            ).ToKeyValuePairs();
            ServiceProvider serviceProvider = ConfigurationTestsHelper
                .ConfigureOptions<VerifiedIdRequirementOptions>(configurationData, VerifiedIdRequirementOptions.SectionName, sectionRequired: false);

            var options = serviceProvider.GetRequiredService<IOptions<VerifiedIdRequirementOptions>>().Value;

            Assert.True(options.Enabled);
            Assert.Equal(90, options.ValidityInMinutes);
            Assert.Equal(TimeSpan.FromMinutes(90), options.Validity);
        }

        public static TheoryData<TestConfigurationSection, Type?, string?> GetTestConfigurations()
        {
            return new TheoryData<TestConfigurationSection, Type?, string?>
            {
                // Empty section: feature disabled, defaults apply
                {
                    TestConfigurationSection.Create(),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("VerifiedIdRequirement:Enabled", "false")
                    ),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("VerifiedIdRequirement:Enabled", "true"),
                        ("VerifiedIdRequirement:ValidityInMinutes", "1")
                    ),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("VerifiedIdRequirement:Enabled", "true"),
                        ("VerifiedIdRequirement:ValidityInMinutes", "10080")
                    ),
                    null,
                    null
                },
                {
                    TestConfigurationSection.Create(
                        ("VerifiedIdRequirement:ValidityInMinutes", "0")
                    ),
                    typeof(OptionsValidationException),
                    "The field 'ValidityInMinutes' must be a positive number of minutes."
                },
                {
                    TestConfigurationSection.Create(
                        ("VerifiedIdRequirement:ValidityInMinutes", "-5")
                    ),
                    typeof(OptionsValidationException),
                    "The field 'ValidityInMinutes' must be a positive number of minutes."
                }
            };
        }
    }
}
