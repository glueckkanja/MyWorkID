using MyWorkID.Server.Common;
using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using NSubstitute;

namespace MyWorkID.Server.IntegrationTests
{
    public class TestApplicationFactory : WebApplicationFactory<IApiAssemblyMarker>
    {
        private Action<IConfigurationBuilder>? _action;

        public void ConfigureConfiguration(Action<IConfigurationBuilder> configure)
        {
            _action += configure;
        }

        protected override IWebHostBuilder? CreateWebHostBuilder()
        {
            if (_action is { } a)
            {
                TestConfiguration.Create(a);
            }

            return base.CreateWebHostBuilder();
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureHostConfiguration(config =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile($"TestSettings/validMinimalSettings.json", optional: false, reloadOnChange: false);
            });

            return base.CreateHost(builder);
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


        public static WebApplicationFactory<T> WithAuthenticationVerifiedId<T>(
            this WebApplicationFactory<T> factory,
            TestClaimsProvider claimsProvider,
            IVerifiedIdSignalRRepository? verifiedIdSignalRRepository = null,
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

                    services.AddScoped(_ => claimsProvider);
                });
            });
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


        public static void AddAuthContextConfig(
            this TestApplicationFactory factory, string appFunction, string? validAuthContextId = null)
        {
            factory.ConfigureConfiguration(cb => cb.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"AppFunctions:{appFunction}"] = validAuthContextId ?? $"c{new Random().Next(1, 100)}"
            }));
        }
    }

}
