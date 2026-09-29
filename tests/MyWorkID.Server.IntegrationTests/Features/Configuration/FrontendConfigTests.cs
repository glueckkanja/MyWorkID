using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.IntegrationTests.Features.Configuration
{
    public class FrontendConfigTests(TestApplicationFactory _testApplicationFactory)
        : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/config/frontend";

        [Fact]
        public async Task GetConfig_WithoutAuth_ReturnsCorrectFrontendConfig()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.GetAsync(
                _baseUrl,
                TestContext.Current.CancellationToken
            );
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var frontendConfigResponse = await response.Content.ReadFromJsonAsync<GetFrontendConfigTestResponse>(
                TestContext.Current.CancellationToken
            );
            frontendConfigResponse.Should().NotBeNull();
            var frontendAppSettings = _testApplicationFactory
                .Services.GetRequiredService<IOptions<FrontendOptions>>()
                .Value;
            frontendAppSettings.BackendClientId.Should().Be(frontendConfigResponse!.BackendClientId);
            frontendAppSettings.FrontendClientId.Should().Be(frontendConfigResponse.FrontendClientId);
            frontendAppSettings.TenantId.Should().Be(frontendConfigResponse.TenantId);
            frontendConfigResponse.MaxDismissibleRiskLevel.Should().Be(RiskLevel.Low.ToString());
            frontendConfigResponse.HelpUrl.Should().BeNull();
        }

        [Fact]
        public async Task GetConfig_WithHelpUrl_ReturnsHelpUrlInResponse()
        {
            var client = _testApplicationFactory
                .WithWebHostBuilder(builder =>
                    builder.ConfigureAppConfiguration(
                        (_, config) =>
                            config.AddInMemoryCollection(
                                new Dictionary<string, string?>
                                {
                                    ["Frontend:HelpUrl"] = "https://example.com/help",
                                }
                            )
                    )
                )
                .CreateDefaultClient();
            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var frontendConfigResponse = await response.Content.ReadFromJsonAsync<GetFrontendConfigTestResponse>(
                TestContext.Current.CancellationToken
            );
            frontendConfigResponse.Should().NotBeNull();
            frontendConfigResponse!.HelpUrl.Should().Be("https://example.com/help");
        }
    }
}
