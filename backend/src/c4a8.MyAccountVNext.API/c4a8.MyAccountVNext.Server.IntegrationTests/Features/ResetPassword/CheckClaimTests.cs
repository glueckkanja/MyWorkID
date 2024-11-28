using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.PasswordReset
{
    public class CheckClaimTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/resetPassword/checkClaim";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public CheckClaimTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
        }

        [Fact]
        public async Task CheckClaim_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CheckClaim_WithWrongRole_Returns403()
        {
            var provider = new TestClaimsProvider().WithDismissUserRiskRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CheckClaim_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task CheckClaim_WithAuthContext_Returns204()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!);
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
