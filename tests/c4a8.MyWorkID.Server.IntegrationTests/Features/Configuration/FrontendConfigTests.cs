using c4a8.MyWorkID.Server.Features.Configuration;
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
            var configuration = _testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            var frontendAppSettings = _testApplicationFactory.Services.GetRequiredService<IOptions<FrontendOptions>>().Value;
            frontendAppSettings.BackendClientId.Should().Be(frontendOptions!.BackendClientId);
            frontendAppSettings.FrontendClientId.Should().Be(frontendOptions.FrontendClientId);
            frontendAppSettings.TenantId.Should().Be(frontendOptions.TenantId);
        }
    }
}
