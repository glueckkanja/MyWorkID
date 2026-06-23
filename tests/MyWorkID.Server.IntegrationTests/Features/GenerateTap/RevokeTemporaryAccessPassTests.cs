using MyWorkID.Server.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace MyWorkID.Server.IntegrationTests.Features.GenerateTap
{
    public class RevokeTemporaryAccessPassTests : IClassFixture<TestApplicationFactory>
    {
        private const string _temporaryAccessPassId = "00000000-0000-0000-0000-000000000001";
        private readonly string _baseUrl = $"/api/me/tap/{_temporaryAccessPassId}";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly string _validAuthContextId;
        private readonly TestApplicationFactory _configuredTestApplicationFactory;

        public RevokeTemporaryAccessPassTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            _validAuthContextId = $"c{new Random().Next(1, 100)}";
            var configuredTestApplicationFactory = new TestApplicationFactory();
            configuredTestApplicationFactory.AddAuthContextConfig(AppFunctions.GenerateTap.ToString(), _validAuthContextId);
            _configuredTestApplicationFactory = configuredTestApplicationFactory;
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithWrongRole_Returns403()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithResetPasswordRole());
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithAuth_WithoutAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithGenerateTapRole());
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_GENERATE_TAP);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithAuth_WithIncorrectAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.GenerateTap.ToString(), "invalid");
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithGenerateTapRole());
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_GENERATE_TAP);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory, provider => provider.WithGenerateTapRole());
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithAuth_WithIncorrectAuthContext_Returns401WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.GenerateTap.ToString(), "c1");
            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithGenerateTapRole().WithAuthContext("c2"));
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithAuthContext_ButNoUserId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithGenerateTapRole().WithAuthContext(_validAuthContextId));
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_WithAuth_Returns204()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithGenerateTapRole().WithRandomSubAndOid().WithAuthContext(_validAuthContextId));
            var response = await client.DeleteAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
