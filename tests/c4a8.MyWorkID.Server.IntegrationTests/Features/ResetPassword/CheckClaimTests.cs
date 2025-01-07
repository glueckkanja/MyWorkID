using c4a8.MyWorkID.Server.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.ResetPassword
{
    public class CheckClaimTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/resetPassword/checkClaim";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly string _validAuthContextId;
        private readonly TestApplicationFactory _configuredTestApplicationFactory;

        public CheckClaimTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuredTestApplicationFactory = new TestApplicationFactory();
            _validAuthContextId = $"c{new Random().Next(1, 100)}";
            configuredTestApplicationFactory.AddAuthContextConfig(AppFunctions.ResetPassword.ToString(), _validAuthContextId);
            _configuredTestApplicationFactory = configuredTestApplicationFactory;
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
        public async Task CheckClaim_WithoutAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithResetPasswordRole());
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_RESET_PASSWORD);
        }

        // Tests if the set auth conext id is valid (c1-c99)
        [Fact]
        public async Task CheckClaim_WithAuth_WithIncorrectAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.ResetPassword.ToString(), "invalid");
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithResetPasswordRole());
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_RESET_PASSWORD);
        }

        [Fact]
        public async Task CheckClaim_WithAuth_WithIncorrectAuthContext_Returns401WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.ResetPassword.ToString(), "c1");
            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithResetPasswordRole().WithAuthContext("c2"));
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }


        [Fact]
        public async Task CheckClaim_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory, provider => provider.WithResetPasswordRole());
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task CheckClaim_WithAuthContext_Returns204()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithResetPasswordRole().WithAuthContext(_validAuthContextId));
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
