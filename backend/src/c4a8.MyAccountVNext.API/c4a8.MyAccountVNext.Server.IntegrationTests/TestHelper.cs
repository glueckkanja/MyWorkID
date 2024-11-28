using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using Microsoft.Kiota.Abstractions;

namespace c4a8.MyAccountVNext.Server.IntegrationTests
{
    public class TestHelper
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
