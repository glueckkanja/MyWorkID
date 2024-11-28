using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.UserRiskState
{
    public class GetUserRiskStateTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/riskstate";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public GetUserRiskStateTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
        }

        [Fact]
        public async Task GetUserRisk_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task GetUserRisk_ButNoUserId_Returns401()
        {
            var provider = new TestClaimsProvider();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserRisk_GraphDoesNotReturnRiskyUser_Returns404()
        {
            var provider = new TestClaimsProvider().WithRandomUserId();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserRisk_GraphReturnsRiskyUserWithoutRisk_ReturnsRiskLevelNone()
        {
            RiskyUser riskyUser = new RiskyUser();
            IRequestAdapter requestAdapter = GetGraphRequestAdapterForRiskyUser(riskyUser);
            var provider = new TestClaimsProvider().WithRandomUserId();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getRiskStateResponse = await response.Content.ReadFromJsonAsync<GetRiskStateTestResponse>();
            getRiskStateResponse?.RiskState.Should().Be(RiskState.None.ToString());
            getRiskStateResponse?.RiskLevel.Should().BeNull();
        }

        [Fact]
        public async Task GetUserRisk_GraphReturnsRiskyUserAtRisk_ReturnsRiskLevelMedium()
        {
            RiskyUser riskyUser = new RiskyUser
            {
                RiskState = RiskState.AtRisk,
                RiskLevel = RiskLevel.Medium
            };
            IRequestAdapter requestAdapter = GetGraphRequestAdapterForRiskyUser(riskyUser);
            var provider = new TestClaimsProvider().WithRandomUserId();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getRiskStateResponse = await response.Content.ReadFromJsonAsync<GetRiskStateTestResponse>();
            getRiskStateResponse?.RiskState.Should().Be(RiskState.None.ToString());
            getRiskStateResponse?.RiskLevel.Should().Be(RiskLevel.Medium.ToString());
        }

        [Fact]
        public async Task GetUserRisk_GraphReturnsRiskyUserRiskStateConfirmedCompromised_ReturnsRiskLevelHigh()
        {
            RiskyUser riskyUser = new RiskyUser
            {
                RiskState = RiskState.ConfirmedCompromised,
                RiskLevel = RiskLevel.High
            };
            IRequestAdapter requestAdapter = GetGraphRequestAdapterForRiskyUser(riskyUser);
            var provider = new TestClaimsProvider().WithRandomUserId();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var getRiskStateResponse = await response.Content.ReadFromJsonAsync<GetRiskStateTestResponse>();
            getRiskStateResponse?.RiskState.Should().Be(RiskState.None.ToString());
            getRiskStateResponse?.RiskLevel.Should().Be(RiskLevel.High.ToString());
        }

        [Fact]
        public async Task GetUserRisk_GraphReturnsNotFoundError_Returns404()
        {
            ODataError oDataError = new ODataError
            {
                ResponseStatusCode = StatusCodes.Status404NotFound
            };
            IRequestAdapter requestAdapter = GetGraphRequestAdapterForException(oDataError);
            var provider = new TestClaimsProvider().WithRandomUserId();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserRisk_GraphReturnsOtherError_Returns500()
        {
            IRequestAdapter requestAdapter = GetGraphRequestAdapterForException(new ODataError());
            var provider = new TestClaimsProvider().WithRandomUserId();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
            var response = await client.GetAsync(_baseUrl);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        private static IRequestAdapter GetGraphRequestAdapterForException(Exception oDataError)
        {
            var requestAdapter = Substitute.For<IRequestAdapter>();
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<RiskyUser>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ThrowsForAnyArgs(oDataError);
            return requestAdapter;
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
