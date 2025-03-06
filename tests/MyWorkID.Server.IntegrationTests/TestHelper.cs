using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Kiota.Abstractions;

namespace MyWorkID.Server.IntegrationTests
{
    public static class TestHelper
    {
        public static HttpClient CreateClientWithTestAuth<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IRequestAdapter? requestAdapter = null) where T : class
        {
            var client = factory.WithAuthentication(claimsProvider, requestAdapter).CreateClient();
            return client;
        }

        public static HttpClient CreateClientWithRole(TestApplicationFactory testApplicationFactory,
            Action<TestClaimsProvider> configureProvider, IRequestAdapter? requestAdapter = null)
        {
            var provider = new TestClaimsProvider();
            configureProvider(provider);
            return testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
        }

        public static HttpClient CreateClientWithTestAuthVerifiedId<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IVerifiedIdSignalRRepository? verifiedIdSignalRRepository = null,
            IHubContext<VerifiedIdHub, IVerifiedIdHub>? hubContext = null,
            IRequestAdapter? requestAdapter = null) where T : class
        {
            var client = factory.WithAuthenticationVerifiedId(
                claimsProvider,
                verifiedIdSignalRRepository,
                hubContext,
                requestAdapter).CreateClient();
            return client;
        }

        internal static Dictionary<string, string?> GetValidVerifiedIdSettings()
        {
            return new Dictionary<string, string?>
            {
                ["VerifiedId:DecentralizedIdentifier"] = "did:web:verifiedid.entra.microsoft.com:d2a8f8b2-3c4e-4b8e-9f2e-1a2b3c4d5e6f:7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d",
                ["VerifiedId:BackendUrl"] = "https://myworkid.glueckkanja.com",
                ["VerifiedId:JwtSigningKey"] = "b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5123",
                ["VerifiedId:TargetSecurityAttributeSet"] = "MyWorkId",
                ["VerifiedId:TargetSecurityAttribute"] = "lastVerifiedFaceCheck",
                ["VerifiedId:CreatePresentationRequestUri"] = "https://verifiedid.did.msidentity.com/v1.0/verifiableCredentials/createPresentationRequest",
            };
        }
    }
}
