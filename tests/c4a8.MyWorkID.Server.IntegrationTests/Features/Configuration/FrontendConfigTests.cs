using c4a8.MyWorkID.Server.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.Configuration
{
    public class FrontendConfigTests(TestApplicationFactory _testApplicationFactory) : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/config/frontend";

        [Fact]
        public async Task GetConfig_WithoutAuth_ReturnsCorrectFrontendConfig()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var frontendOptions = await response.Content.ReadFromJsonAsync<FrontendOptions>();
            frontendOptions.Should().NotBeNull();
            var frontendAppSettings = _testApplicationFactory.Services.GetRequiredService<IOptions<FrontendOptions>>().Value;
            frontendAppSettings.BackendClientId.Should().Be(frontendOptions!.BackendClientId);
            frontendAppSettings.FrontendClientId.Should().Be(frontendOptions.FrontendClientId);
            frontendAppSettings.TenantId.Should().Be(frontendOptions.TenantId);
        }

        [Fact]
        public void MissingBackendClientIdConfiguration_ThrowsValidationError()
        {
            var app = new TestApplicationFactory();
            app.ConfigureConfiguration(cb => cb.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Frontend:BackendClientId"] = "Hello3"
            }));

            try
            {
                app.CreateClient();
            }
            catch (Exception e)
            {
                e.Should().BeOfType<OptionsValidationException>();
                e.Message.Should().Contain("DataAnnotation validation failed for 'FrontendOptions' members: '' with the error: 'The field 'BackendClientId' must be a valid GUID.'.");
            }
        }
    }
}
