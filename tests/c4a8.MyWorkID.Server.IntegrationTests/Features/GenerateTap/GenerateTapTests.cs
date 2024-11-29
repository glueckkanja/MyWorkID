using c4a8.MyWorkID.Server.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.GenerateTap
{
    public class GenerateTapTests : IClassFixture<TestApplicationFactory>
    {
        private const string _baseUrl = "/api/me/generatetap";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public GenerateTapTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
        }

        [Fact]
        public async Task GenerateTap_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GenerateTap_WithWrongRole_Returns403()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithResetPasswordRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GenerateTap_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithGenerateTapRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task GenerateTap_WithAuthContext_ButNoUserId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithGenerateTapRole().WithAuthContext(_appFunctionsOptions.GenerateTap!));
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GenerateTap_WithAuth_Returns500()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithGenerateTapRole().WithRandomSubAndOid().WithAuthContext(_appFunctionsOptions.GenerateTap!));
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_UNABLE_TO_GENERATE_TAP);
        }
    }
}
