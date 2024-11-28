using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.VerifiedId
{
    public class RequestPresentationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/verfiedId/callback";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public RequestPresentationTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
        }

        [Fact]
        public async Task RequestPresentation_WithoutAuth_Returns401()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
