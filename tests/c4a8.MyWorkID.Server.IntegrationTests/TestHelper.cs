using c4a8.MyWorkID.Server.IntegrationTests.Authentication;
using Microsoft.Kiota.Abstractions;

namespace c4a8.MyWorkID.Server.IntegrationTests
{
    public static class TestHelper
    {
        public static HttpClient CreateClientWithRole(TestApplicationFactory testApplicationFactory,
            Action<TestClaimsProvider> configureProvider, IRequestAdapter? requestAdapter = null)
        {
            var provider = new TestClaimsProvider();
            configureProvider(provider);
            return testApplicationFactory.CreateClientWithTestAuth(provider, requestAdapter);
        }
    }
}
