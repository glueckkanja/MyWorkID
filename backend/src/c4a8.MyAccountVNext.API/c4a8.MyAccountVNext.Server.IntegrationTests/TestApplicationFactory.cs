using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using NSubstitute;

namespace c4a8.MyAccountVNext.Server.IntegrationTests
{
    public class TestApplicationFactory : WebApplicationFactory<IApiAssemblyMarker>, IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            return Task.FromResult(0);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            return Task.FromResult(0);
        }
    }

    public static class WebApplicationFactoryExtensions
    {
        public static WebApplicationFactory<T> WithAuthentication<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IRequestAdapter? requestAdapter = null) where T : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    requestAdapter ??= Substitute.For<IRequestAdapter>();
                    var graphServiceClientMock = new GraphServiceClient(requestAdapter);
                    services.AddSingleton(graphServiceClientMock);

                    services
                        .AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                    services.AddScoped(_ => claimsProvider);
                });
            });
        }

        public static HttpClient CreateClientWithTestAuth<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IRequestAdapter? requestAdapter = null) where T : class
        {
            var client = factory.WithAuthentication(claimsProvider, requestAdapter).CreateClient();
            return client;
        }
    }

}
