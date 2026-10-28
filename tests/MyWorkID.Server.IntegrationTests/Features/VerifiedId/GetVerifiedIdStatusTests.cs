using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using MyWorkID.Server.Options;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    public class GetVerifiedIdStatusTests : IClassFixture<TestApplicationFactory>
    {
        private const string _baseUrl = "/api/me/verifiedId/status";
        private readonly TestApplicationFactory _testApplicationFactory;

        public GetVerifiedIdStatusTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
        }

        [Fact]
        public async Task GetStatus_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetStatus_WithoutObjectId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithGenerateTapRole());
            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetStatus_WhenRequirementDisabled_ReturnsNotRequiredWithoutCallingGraph()
        {
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterForUserVerifiedMinutesAgo(5);
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithRandomSubAndOid(), requestAdapter);

            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var status = await response.Content.ReadFromJsonAsync<GetVerifiedIdStatusTestResponse>(TestContext.Current.CancellationToken);
            status.Should().NotBeNull();
            status!.Required.Should().BeFalse();
            status.IsVerified.Should().BeFalse();
            status.LastVerifiedAt.Should().BeNull();
            status.ValidUntil.Should().BeNull();
            status.ValidityInMinutes.Should().Be(VerifiedIdRequirementOptions.DEFAULT_VALIDITY_IN_MINUTES);
            await requestAdapter.DidNotReceiveWithAnyArgs().SendAsync(
                default!,
                Arg.Any<Microsoft.Kiota.Abstractions.Serialization.ParsableFactory<User>>(),
                default,
                TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task GetStatus_WhenRequirementEnabledAndUserNeverVerified_ReturnsRequiredAndNotVerified()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddVerifiedIdRequirementConfig(validityInMinutes: 120);
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(new User());
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithRandomSubAndOid(), requestAdapter);

            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var status = await response.Content.ReadFromJsonAsync<GetVerifiedIdStatusTestResponse>(TestContext.Current.CancellationToken);
            status.Should().NotBeNull();
            status!.Required.Should().BeTrue();
            status.ValidityInMinutes.Should().Be(120);
            status.IsVerified.Should().BeFalse();
            status.LastVerifiedAt.Should().BeNull();
            status.ValidUntil.Should().BeNull();
        }

        [Fact]
        public async Task GetStatus_WhenRequirementEnabledAndUserRecentlyVerified_ReturnsVerifiedWithTimestamps()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddVerifiedIdRequirementConfig(validityInMinutes: 120);
            DateTimeOffset lastVerifiedAt = DateTimeOffset.UtcNow.AddMinutes(-30);
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(
                VerifiedIdRequirementTestHelper.CreateUserWithVerification(lastVerifiedAt));
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithRandomSubAndOid(), requestAdapter);

            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var status = await response.Content.ReadFromJsonAsync<GetVerifiedIdStatusTestResponse>(TestContext.Current.CancellationToken);
            status.Should().NotBeNull();
            status!.Required.Should().BeTrue();
            status.IsVerified.Should().BeTrue();
            status.LastVerifiedAt.Should().BeCloseTo(lastVerifiedAt, TimeSpan.FromSeconds(1));
            status.ValidUntil.Should().BeCloseTo(lastVerifiedAt.AddMinutes(120), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetStatus_WhenRequirementEnabledAndVerificationExpired_ReturnsNotVerifiedWithTimestamps()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddVerifiedIdRequirementConfig(validityInMinutes: 60);
            DateTimeOffset lastVerifiedAt = DateTimeOffset.UtcNow.AddMinutes(-90);
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(
                VerifiedIdRequirementTestHelper.CreateUserWithVerification(lastVerifiedAt));
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithRandomSubAndOid(), requestAdapter);

            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var status = await response.Content.ReadFromJsonAsync<GetVerifiedIdStatusTestResponse>(TestContext.Current.CancellationToken);
            status.Should().NotBeNull();
            status!.Required.Should().BeTrue();
            status.IsVerified.Should().BeFalse();
            status.LastVerifiedAt.Should().BeCloseTo(lastVerifiedAt, TimeSpan.FromSeconds(1));
            status.ValidUntil.Should().BeCloseTo(lastVerifiedAt.AddMinutes(60), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetStatus_WhenRequirementEnabledWithoutTargetAttributeConfig_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddVerifiedIdRequirementConfig(configureTargetSecurityAttribute: false);
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithRandomSubAndOid());

            var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID_REQUIREMENT);
        }
    }
}
