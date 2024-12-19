using c4a8.MyWorkID.Server.Features.VerifiedId;
using c4a8.MyWorkID.Server.Features.VerifiedId.SignalR;
using c4a8.MyWorkID.Server.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using NSubstitute;

namespace c4a8.MyWorkID.Server.IntegrationTests
{
    public class TestApplicationFactory : WebApplicationFactory<IApiAssemblyMarker>, IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            return Task.FromResult(0);
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
                        .AddAuthentication(TestAuthHandler.TestScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });

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

        public static WebApplicationFactory<T> WithAuthenticationVerifiedId<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IVerifiedIdSignalRRepository? verifiedIdSignalRRepository = null,
            VerifiedIdOptions? verifiedIdOptions = null,
            IHubContext<VerifiedIdHub, IVerifiedIdHub>? hubContext = null,
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
                        .AddAuthentication(TestAuthHandler.TestScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });

                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy(Strings.VERIFIED_ID_CALLBACK_SCHEMA, policy =>
                        {
                            policy.RequireAuthenticatedUser();
                            policy.AuthenticationSchemes.Add(TestAuthHandler.TestScheme);
                        });
                    });

                    if (verifiedIdSignalRRepository != null)
                    {
                        services.AddSingleton(verifiedIdSignalRRepository);
                    }

                    if (hubContext != null)
                    {
                        services.AddSingleton(hubContext);
                    }

                    if (verifiedIdOptions != null)
                    {
                        services.AddSingleton(verifiedIdOptions);
                        var options = Options.Create(verifiedIdOptions);
                        services.AddSingleton(options);
                    }

                    services.AddScoped(_ => claimsProvider);
                });
            });
        }

        public static HttpClient CreateClientWithTestAuthVerifiedId<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IVerifiedIdSignalRRepository? verifiedIdSignalRRepository = null,
            VerifiedIdOptions? verifiedIdOptions = null,
            IHubContext<VerifiedIdHub, IVerifiedIdHub>? hubContext = null,
            IRequestAdapter? requestAdapter = null) where T : class
        {
            var client = factory.WithAuthenticationVerifiedId(
                claimsProvider,
                verifiedIdSignalRRepository,
                verifiedIdOptions,
                hubContext,
                requestAdapter).CreateClient();
            return client;
        }

        public static WebApplicationFactory<T> WithHttpMock<T>(
            this WebApplicationFactory<T> factory,
            MockHttpMessageHandler mockHttpMessageHandler) where T : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    RemoveRegisteredHttpFactoryAndClient(services);
                    AddNewFactoryWithMockedClient(mockHttpMessageHandler, services);
                });
            });
        }

        private static void AddNewFactoryWithMockedClient(MockHttpMessageHandler mockHttpMessageHandler, IServiceCollection services)
        {
            var client = new HttpClient(mockHttpMessageHandler);
            var clientFactoryMock = Substitute.For<IHttpClientFactory>();
            clientFactoryMock
                .CreateClient(Arg.Any<string>())
                .Returns(client);
            services.AddSingleton(clientFactoryMock);
        }

        private static void RemoveRegisteredHttpFactoryAndClient(IServiceCollection services)
        {
            var httpClientFactoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory));
            if (httpClientFactoryDescriptor != null)
            {
                services.Remove(httpClientFactoryDescriptor);
            }

            var httpClientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient));
            if (httpClientDescriptor != null)
            {
                services.Remove(httpClientDescriptor);
            }
        }
    }

}
