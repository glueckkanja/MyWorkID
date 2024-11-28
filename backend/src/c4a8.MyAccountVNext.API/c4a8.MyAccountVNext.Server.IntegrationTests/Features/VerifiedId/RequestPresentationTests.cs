using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.Features.VerifiedId;
using c4a8.MyAccountVNext.Server.Features.VerifiedId.Entities;
using c4a8.MyAccountVNext.Server.Features.VerifiedId.SignalR;
using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.VerifiedId
{
    public class RequestPresentationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/verifiedid/callback";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;
        private readonly VerifiedIdOptions _verifiedIdOptions;

        public RequestPresentationTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
            _verifiedIdOptions = testApplicationFactory.Services.GetRequiredService<IOptions<VerifiedIdOptions>>().Value;
        }

        [Fact]
        public async Task RequestPresentation_WithoutUserId_Returns401()
        {
            var provider = new TestClaimsProvider();
            var client = _testApplicationFactory.WithAuthenticationVerifiedId(provider).CreateClient();
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task RequestPresentation_WithoutBody_Returns400()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext).CreateClient();
            var response = await client.PostAsync(_baseUrl, null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().Be($"\"{Strings.ERROR_INVALID_BODY}\"");
        }

        [Fact]
        public async Task RequestPresentation_WithRequestStatusRequestRetrieved_Returns204()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext).CreateClient();
            var createPresentationRequestCallback = new CreatePresentationRequestCallback
            {
                RequestStatus = "request_retrieved"
            };
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RequestPresentation_WithRequestPresentationError_Returns204()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext).CreateClient();
            var createPresentationRequestCallback = new CreatePresentationRequestCallback
            {
                RequestStatus = "presentation_error",
                Error = new CreatePresentationRequestError
                {
                    Code = "error_code",
                    Message = "error_message"
                }
            };
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RequestPresentation_WithInvalidState_Returns500()
        {
            string userId = Guid.NewGuid().ToString();
            var provider = new TestClaimsProvider().WithUserId(userId);
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext).CreateClient();
            var createPresentationRequestCallback = new CreatePresentationRequestCallback
            {
                RequestStatus = "presentation_verified",
                State = Guid.NewGuid().ToString(),
            };
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task RequestPresentation_WithInvalidTargetSecurityAttribute_Returns204()
        {
            string userId = Guid.NewGuid().ToString();
            var provider = new TestClaimsProvider().WithUserId(userId);
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;
            _verifiedIdOptions.TargetSecurityAttributeSet = "TargetSecurityAttributeSet";
            _verifiedIdOptions.TargetSecurityAttribute = string.Empty;

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext).CreateClient();
            CreatePresentationRequestCallback createPresentationRequestCallback = GetValidPresentationRequestCallback(userId);
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RequestPresentation_WithInvalidTargetSecurityAttributeSet_Returns204()
        {
            string userId = Guid.NewGuid().ToString();
            var provider = new TestClaimsProvider().WithUserId(userId);
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;
            _verifiedIdOptions.TargetSecurityAttributeSet = string.Empty;
            _verifiedIdOptions.TargetSecurityAttribute = "TargetSecurityAttribute";

            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext).CreateClient();
            CreatePresentationRequestCallback createPresentationRequestCallback = GetValidPresentationRequestCallback(userId);
            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task RequestPresentation_WithValidBody_Returns204()
        {
            string userId = Guid.NewGuid().ToString();
            var provider = new TestClaimsProvider().WithUserId(userId);
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository = GetVerifiedSignalRRepositoryWithConnections();
            _verifiedIdOptions.DisableQrCodeHide = false;
            _verifiedIdOptions.TargetSecurityAttributeSet = "TargetSecurityAttribute";
            _verifiedIdOptions.TargetSecurityAttribute = "TargetSecurityAttribute";
            var requestAdapter = Substitute.For<IRequestAdapter>();
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<User>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(new User());
            var hubContext = Substitute.For<IHubContext<VerifiedIdHub, IVerifiedIdHub>>();

            var client = _testApplicationFactory.WithAuthenticationVerifiedId(
                provider,
                verifiedIdSignalRRepository,
                _verifiedIdOptions,
                hubContext,
                requestAdapter).CreateClient();

            CreatePresentationRequestCallback createPresentationRequestCallback = GetValidPresentationRequestCallback(userId);

            var response = await client.PostAsJsonAsync(_baseUrl, createPresentationRequestCallback);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await requestAdapter.Received(1).SendAsync(
            Arg.Is<RequestInformation>(ri => ri.PathParameters.Values.Contains(userId) && ri.HttpMethod == Method.PATCH),
            Arg.Any<ParsableFactory<User>>(),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>());
        }

        private static CreatePresentationRequestCallback GetValidPresentationRequestCallback(string userId)
        {
            return new CreatePresentationRequestCallback
            {
                RequestStatus = "presentation_verified",
                State = userId,
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
