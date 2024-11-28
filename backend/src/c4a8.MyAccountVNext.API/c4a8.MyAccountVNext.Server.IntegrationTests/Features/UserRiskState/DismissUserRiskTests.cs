using c4a8.MyAccountVNext.Server.Common;
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
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithDismissUserRiskRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task DismissUserRisk_WithWrongRole_Returns403()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithResetPasswordRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DismissUserRisk_WithAuthContext_ButNoUserId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithDismissUserRiskRole().WithAuthContext(_appFunctionsOptions.DismissUserRisk!));
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DismissUserRisk_WithRandomUserId_Returns500()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithDismissUserRiskRole().WithRandomSubAndOid().WithAuthContext(_appFunctionsOptions.DismissUserRisk!));
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
