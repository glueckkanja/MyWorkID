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
            var provider = new TestClaimsProvider().WithValidateIdentityRole().WithRandomSubAndOid();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PostAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createPresentationResponse = await response.Content.ReadFromJsonAsync<CreatePresentationResponse>();
            ConvertUnixEpochToDateTime((long)createPresentationResponse!.ExpiryDate!).Should().BeAfter(DateTime.UtcNow);
            createPresentationResponse.QrCodeBase64.Should().NotBeNullOrEmpty();
            createPresentationResponse.RequestId.Should().NotBeNullOrEmpty();
            string pattern = $@"^openid-vc:\/\/\?request_uri=https:\/\/verifiedid\.did\.msidentity\.com\/v1\.0\/tenants\/.*\/verifiableCredentials\/presentationRequests\/{createPresentationResponse.RequestId}$";
            createPresentationResponse.Url.Should().MatchRegex(pattern);
        }

        private DateTime ConvertUnixEpochToDateTime(long unixTime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            return dateTime;
        }
    }
}
