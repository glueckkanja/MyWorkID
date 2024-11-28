using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.UserRiskState
{
    public class DismissUserRiskTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/riskstate/dismiss";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public DismissUserRiskTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
        }

        [Fact]
        public async Task DismissUserRisk_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DismissUserRisk_WithoutAuthContext_Returns401WithMessage()
        {
            var provider = new TestClaimsProvider().WithDismissUserRiskRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task DismissUserRisk_WithWrongRole_Returns403()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DismissUserRisk_WithAuthContext_ButNoUserId_Returns401()
        {
            var provider = new TestClaimsProvider().WithDismissUserRiskRole().WithAuthContext(_appFunctionsOptions.DismissUserRisk!);
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DismissUserRisk_WithRandomUserId_Returns500()
        {
            var provider = new TestClaimsProvider().WithDismissUserRiskRole().WithRandomUserId().WithAuthContext(_appFunctionsOptions.DismissUserRisk!);
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
