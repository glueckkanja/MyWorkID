using c4a8.MyAccountVNext.Server.Common;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.VerifiedId
{
    public class RequestPresentationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/verifiedid/callback";
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
            var jwtToken = JwtTokenGenerator.GenerateTestJwtToken();
            var client = _testApplicationFactory.WithJwtBearerAuthentication(jwtToken).CreateClient();
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
