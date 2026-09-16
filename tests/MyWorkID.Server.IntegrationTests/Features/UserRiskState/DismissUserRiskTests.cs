using MyWorkID.Server.Common;
using MyWorkID.Server.Features.ResetPassword.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace MyWorkID.Server.IntegrationTests.Features.UserRiskState
{
    public class DismissUserRiskTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/riskstate/dismiss";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly string _validAuthContextId;
        private readonly TestApplicationFactory _configuredTestApplicationFactory;

        public DismissUserRiskTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuredTestApplicationFactory = new TestApplicationFactory();
            _validAuthContextId = $"c{new Random().Next(1, 100)}";
            configuredTestApplicationFactory.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), _validAuthContextId);
            _configuredTestApplicationFactory = configuredTestApplicationFactory;
        }

        [Fact]
        public async Task DismissUserRisk_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DismissUserRisk_WithWrongRole_Returns403()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithResetPasswordRole());
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DismissUserRisk_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory, provider => provider.WithDismissUserRiskRole());
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task DismissUserRisk_WithoutAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithDismissUserRiskRole());
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_DISMISS_USER_RISK);
        }

        // Tests if the set auth conext id is valid (c1-c99)
        [Fact]
        public async Task DismissUserRisk_WithAuth_WithIncorrectAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), "invalid");
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithDismissUserRiskRole());
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_DISMISS_USER_RISK);
        }

        [Fact]
        public async Task DismissUserRisk_WithAuth_WithIncorrectAuthContext_Returns401WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), "c1");
            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithDismissUserRiskRole().WithAuthContext("c2"));
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task DismissUserRisk_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory, provider => provider.WithDismissUserRiskRole());
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task DismissUserRisk_WithAuthContext_ButNoUserId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithDismissUserRiskRole().WithAuthContext(_validAuthContextId));
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DismissUserRisk_WithRandomUserId_Returns500()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithDismissUserRiskRole().WithRandomSubAndOid().WithAuthContext(_validAuthContextId));
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DismissUserRisk_WhenUserRiskLevelExceedsConfiguredMax_Returns403()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), _validAuthContextId);
            testApp.AddMaxDismissibleRiskLevelConfig(RiskLevel.Low.ToString());

            IRequestAdapter requestAdapter = GetGraphRequestAdapterForRiskyUser(
                new RiskyUser { RiskState = RiskState.AtRisk, RiskLevel = RiskLevel.High });

            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithDismissUserRiskRole().WithRandomSubAndOid().WithAuthContext(_validAuthContextId),
                requestAdapter);
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_RISK_LEVEL_EXCEEDS_MAX_DISMISSIBLE);
        }

        [Fact]
        public async Task DismissUserRisk_WhenUserRiskLevelWithinConfiguredMax_Returns200()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), _validAuthContextId);
            testApp.AddMaxDismissibleRiskLevelConfig(RiskLevel.High.ToString());

            IRequestAdapter requestAdapter = GetGraphRequestAdapterForRiskyUser(
                new RiskyUser { RiskState = RiskState.AtRisk, RiskLevel = RiskLevel.Medium });

            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithDismissUserRiskRole().WithRandomSubAndOid().WithAuthContext(_validAuthContextId),
                requestAdapter);
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DismissUserRisk_WhenUserRiskLevelEqualsConfiguredMax_Returns200()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), _validAuthContextId);
            testApp.AddMaxDismissibleRiskLevelConfig(RiskLevel.Medium.ToString());

            IRequestAdapter requestAdapter = GetGraphRequestAdapterForRiskyUser(
                new RiskyUser { RiskState = RiskState.AtRisk, RiskLevel = RiskLevel.Medium });

            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithDismissUserRiskRole().WithRandomSubAndOid().WithAuthContext(_validAuthContextId),
                requestAdapter);
            var response = await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DismissUserRisk_WithInvalidMaxDismissibleRiskLevelConfig_FailsStartup()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.DismissUserRisk.ToString(), _validAuthContextId);
            testApp.AddMaxDismissibleRiskLevelConfig("None");

            Func<Task> act = async () =>
            {
                var client = TestHelper.CreateClientWithRole(testApp,
                    provider => provider.WithDismissUserRiskRole().WithRandomSubAndOid().WithAuthContext(_validAuthContextId));
                await client.PutAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            };
            await act.Should().ThrowAsync<Microsoft.Extensions.Options.OptionsValidationException>();
        }

        private static IRequestAdapter GetGraphRequestAdapterForRiskyUser(RiskyUser riskyUser)
        {
            var requestAdapter = Substitute.For<IRequestAdapter>();
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<RiskyUser>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(riskyUser);
            return requestAdapter;
        }
    }
}
