using AutoFixture;
using c4a8.MyWorkID.Server.Features.VerifiedId.Entities;
using c4a8.MyWorkID.Server.IntegrationTests.Authentication;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    public class ValidateIdentityTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/verifiedId/verify";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly Fixture _fixture = new();

        public ValidateIdentityTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
        }

        [Fact]
        public async Task ValidateIdentity_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidateIdentity_WithWrongRole_Returns403()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ValidateIdentity_WithoutUserId_Returns401()
        {
            var provider = new TestClaimsProvider().WithValidateIdentityRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ValidateIdentity_Returns204_WithPresentation()
        {
            var expectedPresentation = new CreatePresentationResponse(
                requestId: Guid.NewGuid().ToString(),
                expiryDate: DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
                qrCodeBase64: _fixture.Create<string>(),
                url: _fixture.Create<string>());

            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, expectedPresentation);
            var provider = new TestClaimsProvider().WithValidateIdentityRole().WithRandomSubAndOid();
            var client = _testApplicationFactory.WithHttpMock(handler).CreateClientWithTestAuth(provider);
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createPresentationResponse = await response.Content.ReadFromJsonAsync<CreatePresentationResponse>();
            ConvertUnixEpochToDateTime((long)createPresentationResponse!.ExpiryDate!).Should().BeAfter(DateTime.UtcNow);
            createPresentationResponse.QrCodeBase64.Should().Be(expectedPresentation.QrCodeBase64);
            createPresentationResponse.RequestId.Should().Be(expectedPresentation.RequestId);
            createPresentationResponse.Url.Should().Be(expectedPresentation.Url);
            createPresentationResponse.ExpiryDate.Should().Be(expectedPresentation.ExpiryDate);
        }


        [Fact]
        public async Task ValidateIdentity_Returns500_WithoutPresentation()
        {
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, null);
            var provider = new TestClaimsProvider().WithValidateIdentityRole().WithRandomSubAndOid();
            var client = _testApplicationFactory.WithHttpMock(handler).CreateClientWithTestAuth(provider);
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        private static DateTime ConvertUnixEpochToDateTime(long unixTime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            return dateTime;
        }
    }
}
