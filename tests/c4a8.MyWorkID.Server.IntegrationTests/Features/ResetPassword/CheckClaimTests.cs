using c4a8.MyWorkID.Server.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.ResetPassword
{
    public class CheckClaimTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/resetPassword/checkClaim";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public CheckClaimTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
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
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithDismissUserRiskRole());
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CheckClaim_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithResetPasswordRole());
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task CheckClaim_WithAuthContext_Returns204()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!));
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
