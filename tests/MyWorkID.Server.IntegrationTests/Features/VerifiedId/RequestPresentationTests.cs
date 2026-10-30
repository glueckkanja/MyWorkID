using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    public class RequestPresentationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/verifiedid/callback";
        private readonly TestApplicationFactory _testApplicationFactory;

        public RequestPresentationTests(TestApplicationFactory testApplicationFactory)
        {
            testApplicationFactory.ConfigureConfiguration(cb => cb.AddInMemoryCollection(TestHelper.GetValidVerifiedIdSettings()));
            _testApplicationFactory = testApplicationFactory;
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithoutAppSetting_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var client = testApp.WithAuthenticationVerifiedId(provider).CreateClient();
            var response = await client.PostAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Contain(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID);
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithMissingDecentralizedIdentifier_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var validSettings = TestHelper.GetValidVerifiedIdSettings();
            validSettings.Remove("VerifiedId:DecentralizedIdentifier");
            var problemDetails = await GetVerifiedIdOptionsProblemDetails(provider, testApp, validSettings);
            problemDetails.Detail.Should().Contain("The DecentralizedIdentifier field is required.");
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithMissingBackendUrl_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var validSettings = TestHelper.GetValidVerifiedIdSettings();
            validSettings.Remove("VerifiedId:BackendUrl");
            var problemDetails = await GetVerifiedIdOptionsProblemDetails(provider, testApp, validSettings);
            problemDetails.Detail.Should().Contain("The BackendUrl field is required.");
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithMissingJwtSigningKey_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var validSettings = TestHelper.GetValidVerifiedIdSettings();
            validSettings.Remove("VerifiedId:JwtSigningKey");
            var problemDetails = await GetVerifiedIdOptionsProblemDetails(provider, testApp, validSettings);
            problemDetails.Detail.Should().Contain("The JwtSigningKey field is required.");
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithMissingTargetSecurityAttributeSet_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var validSettings = TestHelper.GetValidVerifiedIdSettings();
            validSettings.Remove("VerifiedId:TargetSecurityAttributeSet");
            var problemDetails = await GetVerifiedIdOptionsProblemDetails(provider, testApp, validSettings);
            problemDetails.Detail.Should().Contain("The TargetSecurityAttributeSet field is required.");
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithMissingTargetSecurityAttribute_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var validSettings = TestHelper.GetValidVerifiedIdSettings();
            validSettings.Remove("VerifiedId:TargetSecurityAttribute");
            var problemDetails = await GetVerifiedIdOptionsProblemDetails(provider, testApp, validSettings);
            problemDetails.Detail.Should().Contain("The TargetSecurityAttribute field is required.");
        }

        [Fact]
        public async Task RequestPresentation_WithAuth_WithMissingCreatePresentationRequestUri_Returns500WithMessage()
        {
            var provider = new TestClaimsProvider();
            var testApp = new TestApplicationFactory();
            var validSettings = TestHelper.GetValidVerifiedIdSettings();
            validSettings.Remove("VerifiedId:CreatePresentationRequestUri");
            var problemDetails = await GetVerifiedIdOptionsProblemDetails(provider, testApp, validSettings);
            problemDetails.Detail.Should().Contain("The CreatePresentationRequestUri field is required.");
        }

        private async Task<ValidationProblemDetails> GetVerifiedIdOptionsProblemDetails(TestClaimsProvider provider, TestApplicationFactory testApp, Dictionary<string, string?> validSettings)
        {
            testApp.ConfigureConfiguration(cb => cb.AddInMemoryCollection(validSettings));
            var client = testApp.WithAuthenticationVerifiedId(provider).CreateClient();
            var response = await client.PostAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            return problemDetails!;
        }

        [Fact]
        public async Task RequestPresentation_WithoutUserId_Returns401()
        {
            var provider = new TestClaimsProvider();
            var client = _testApplicationFactory.WithAuthenticationVerifiedId(provider).CreateClient();
            var response = await client.PostAsync(_baseUrl, null, TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RequestPresentation_WithoutBody_Returns400()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                hubContext).CreateClient();
            var response = await client.PostAsync(_baseUrl, null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RequestPresentation_WithRequestStatusRequestRetrieved_Returns204()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                hubContext).CreateClient();
            var createPresentationRequestCallback = new CreatePresentationRequestCallback
            {
                RequestStatus = "request_retrieved"
            };
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RequestPresentation_WithRequestPresentationError_Returns204()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                hubContext).CreateClient();
            var createPresentationRequestCallback = new CreatePresentationRequestCallback
            {
                RequestStatus = "presentation_error",

            };
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RequestPresentation_WithInvalidState_Returns500()
        {
            string userId = Guid.NewGuid().ToString();
            var provider = new TestClaimsProvider().WithUserId(userId);
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                hubContext).CreateClient();
            var createPresentationRequestCallback = new CreatePresentationRequestCallback
            {
                RequestStatus = "presentation_verified",
                State = Guid.NewGuid().ToString(),
            };
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task RequestPresentation_WithValidBody_Returns204()
        {
            string userId = Guid.NewGuid().ToString();
            var provider = new TestClaimsProvider().WithUserId(userId);
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            IRequestAdapter requestAdapter = CreateRequestAdapterReturningUserPrincipalName(UserPrincipalName);
            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                hubContext,
                requestAdapter).CreateClient();

            CreatePresentationRequestCallback createPresentationRequestCallback = GetValidPresentationRequestCallback(userId);

            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await requestAdapter.Received(1).SendAsync(
            Arg.Is<RequestInformation>(ri => ri.PathParameters.Values.Contains(userId) && ri.HttpMethod == Method.PATCH),
            Arg.Any<ParsableFactory<User>>(),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RequestPresentation_WithUserPrincipalNameInDifferentCasing_RecordsVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);
            callback.VerifiedCredentialsData!.Single().Claims![Strings.VERIFIED_ID_USER_PRINCIPAL_NAME_CLAIM] = UserPrincipalName.ToUpperInvariant();

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, UserPrincipalName);

            await AssertVerificationRecorded(requestAdapter, userId, expected: true);
        }

        [Fact]
        public async Task RequestPresentation_WithCredentialOfOtherUser_DoesNotRecordVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);
            callback.VerifiedCredentialsData!.Single().Claims![Strings.VERIFIED_ID_USER_PRINCIPAL_NAME_CLAIM] = "attacker@contoso.com";

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, UserPrincipalName);

            await AssertVerificationRecorded(requestAdapter, userId, expected: false);
        }

        [Fact]
        public async Task RequestPresentation_WithCredentialWithoutUserPrincipalName_DoesNotRecordVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);
            callback.VerifiedCredentialsData!.Single().Claims!.Remove(Strings.VERIFIED_ID_USER_PRINCIPAL_NAME_CLAIM);

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, UserPrincipalName);

            await AssertVerificationRecorded(requestAdapter, userId, expected: false);
        }

        [Fact]
        public async Task RequestPresentation_WithUntrustedIssuer_DoesNotRecordVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);
            callback.VerifiedCredentialsData!.Single().Issuer = "did:web:untrusted.example.com";

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, UserPrincipalName);

            await AssertVerificationRecorded(requestAdapter, userId, expected: false);
        }

        [Fact]
        public async Task RequestPresentation_WithOtherCredentialType_DoesNotRecordVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);
            callback.VerifiedCredentialsData!.Single().Type = ["VerifiableCredential", "SomeOtherCredential"];

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, UserPrincipalName);

            await AssertVerificationRecorded(requestAdapter, userId, expected: false);
        }

        [Fact]
        public async Task RequestPresentation_WithoutVerifiedCredentialsData_DoesNotRecordVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);
            callback.VerifiedCredentialsData = null;

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, UserPrincipalName);

            await AssertVerificationRecorded(requestAdapter, userId, expected: false);
        }

        [Fact]
        public async Task RequestPresentation_WhenUserPrincipalNameCannotBeResolved_DoesNotRecordVerification()
        {
            string userId = Guid.NewGuid().ToString();
            CreatePresentationRequestCallback callback = GetValidPresentationRequestCallback(userId);

            IRequestAdapter requestAdapter = await PostCallback(userId, callback, userPrincipalName: null);

            await AssertVerificationRecorded(requestAdapter, userId, expected: false);
        }

        private const string UserPrincipalName = "jane.doe@contoso.com";

        private async Task<IRequestAdapter> PostCallback(
            string userId,
            CreatePresentationRequestCallback callback,
            string? userPrincipalName)
        {
            var provider = new TestClaimsProvider().WithUserId(userId);
            IRequestAdapter requestAdapter = CreateRequestAdapterReturningUserPrincipalName(userPrincipalName);
            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                GetVerifiedSignalRRepositoryWithConnections(),
                Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>(),
                requestAdapter).CreateClient();

            var response = await client.PostAsJsonAsync(_baseUrl, callback, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            return requestAdapter;
        }

        private static async Task AssertVerificationRecorded(IRequestAdapter requestAdapter, string userId, bool expected)
        {
            await requestAdapter.Received(expected ? 1 : 0).SendAsync(
                Arg.Is<RequestInformation>(ri => ri.PathParameters.Values.Contains(userId) && ri.HttpMethod == Method.PATCH),
                Arg.Any<ParsableFactory<User>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>());
        }

        private static IRequestAdapter CreateRequestAdapterReturningUserPrincipalName(string? userPrincipalName)
        {
            var requestAdapter = Substitute.For<IRequestAdapter>();
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<User>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(new User { UserPrincipalName = userPrincipalName });
            return requestAdapter;
        }

        private static CreatePresentationRequestCallback GetValidPresentationRequestCallback(string userId)
        {
            return new CreatePresentationRequestCallback
            {
                RequestStatus = "presentation_verified",
                State = userId,
                VerifiedCredentialsData =
                [
                    new VerifiedCredentialsData
                    {
                        Issuer = TestHelper.GetValidVerifiedIdSettings()["VerifiedId:DecentralizedIdentifier"],
                        Type = ["VerifiableCredential", Strings.VERIFIED_ID_VERIFIED_EMPLOYEE_CREDENTIAL_TYPE],
                        Claims = new Dictionary<string, string>
                        {
                            [Strings.VERIFIED_ID_USER_PRINCIPAL_NAME_CLAIM] = UserPrincipalName,
                        },
                    },
                ],
            };
        }

        private static IVerifiedIdSignalRRepository GetVerifiedSignalRRepositoryWithConnections()
        {
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = Substitute.For<IVerifiedIdSignalRRepository>();
            verifiedIdSignalRRepository.TryGetConnections(Arg.Any<string>(), out Arg.Any<HashSet<string>>()!)
                .Returns(callInfo =>
                {
                    callInfo[1] = new HashSet<string> { "connectionId1", "connectionId2" };
                    return true;
                });
            return verifiedIdSignalRRepository;
        }
    }
}
