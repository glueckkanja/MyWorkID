using System.Net;
using FluentAssertions;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.IntegrationTests.Authentication;

namespace MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    public class DiscardVerificationTests
    {
        private const string BaseUrl = "/api/me/verifiedId/discard";
        private static readonly DateTimeOffset _now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task DiscardVerification_WithoutAuth_Returns401()
        {
            var client = new TestApplicationFactory().CreateDefaultClient();

            var response = await client.PostAsync($"{BaseUrl}?action=GenerateTemporaryAccessPass", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DiscardVerification_WithWrongRole_Returns403()
        {
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(null);
            var client = CreateClient(requestAdapter, new TestClaimsProvider().WithResetPasswordRole().WithRandomSubAndOid());

            var response = await client.PostAsync($"{BaseUrl}?action=GenerateTemporaryAccessPass", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Unknown")]
        [InlineData("generateTemporaryAccessPass")]
        public async Task DiscardVerification_WithUnknownAction_Returns400WithoutCallingGraph(string action)
        {
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(null);
            var client = CreateClient(requestAdapter);

            var response = await client.PostAsync($"{BaseUrl}?action={action}", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter).Should().BeEmpty();
        }

        [Theory]
        [InlineData(VerifiedIdAction.GenerateTemporaryAccessPass)]
        [InlineData(VerifiedIdAction.ResetPassword)]
        [InlineData(VerifiedIdAction.DismissUserRisk)]
        public async Task DiscardVerification_WhenVerificationIsBoundToAction_MarksItAsUsed(VerifiedIdAction action)
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(_now.AddMinutes(-1), action));
            var client = CreateClient(requestAdapter);

            var response = await client.PostAsync($"{BaseUrl}?action={action}", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter)
                .Where(VerifiedIdRequirementTestHelper.IsVerificationMarkedAsUsed).Should().ContainSingle();
        }

        [Fact]
        public async Task DiscardVerification_WhenVerificationIsBoundToOtherAction_KeepsIt()
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(_now.AddMinutes(-1), VerifiedIdAction.ResetPassword));
            var client = CreateClient(requestAdapter);

            var response = await client.PostAsync($"{BaseUrl}?action=GenerateTemporaryAccessPass", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter).Should().NotContain(request => request.HttpMethod == Method.PATCH);
        }

        [Fact]
        public async Task DiscardVerification_WhenVerificationIsNotBoundToAnAction_KeepsIt()
        {
            // A verification from the standalone identity validation stays visible to administrators.
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(_now.AddMinutes(-1)));
            var client = CreateClient(requestAdapter);

            var response = await client.PostAsync($"{BaseUrl}?action=GenerateTemporaryAccessPass", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter).Should().NotContain(request => request.HttpMethod == Method.PATCH);
        }

        [Theory]
        [InlineData(false, 0)]
        [InlineData(true, 60)]
        public async Task DiscardVerification_WhenNotInPerActionMode_DoesNothing(bool requirementEnabled, int validityInMinutes)
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(_now.AddMinutes(-1), VerifiedIdAction.GenerateTemporaryAccessPass));
            var client = CreateClient(requestAdapter, requirementEnabled: requirementEnabled, validityInMinutes: validityInMinutes);

            var response = await client.PostAsync($"{BaseUrl}?action=GenerateTemporaryAccessPass", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter).Should().BeEmpty();
        }

        [Fact]
        public async Task DiscardVerification_WithoutTargetAttributeConfiguration_Returns500()
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(null);
            var client = CreateClient(requestAdapter, configureTargetSecurityAttribute: false);

            var response = await client.PostAsync($"{BaseUrl}?action=GenerateTemporaryAccessPass", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        private static IRequestAdapter CreateRequestAdapter(User? user)
        {
            return VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(user).WithJsonSerialization();
        }

        private static HttpClient CreateClient(
            IRequestAdapter requestAdapter,
            TestClaimsProvider? claimsProvider = null,
            bool requirementEnabled = true,
            int validityInMinutes = 0,
            bool configureTargetSecurityAttribute = true)
        {
            var testApplicationFactory = new TestApplicationFactory();
            testApplicationFactory.AddVerifiedIdRequirementConfig(requirementEnabled, validityInMinutes, configureTargetSecurityAttribute);

            return testApplicationFactory
                .WithAuthentication(claimsProvider ?? new TestClaimsProvider().WithValidateIdentityRole().WithRandomSubAndOid(), requestAdapter)
                .WithTimeProvider(new FixedTimeProvider(_now))
                .CreateClient();
        }
    }
}
