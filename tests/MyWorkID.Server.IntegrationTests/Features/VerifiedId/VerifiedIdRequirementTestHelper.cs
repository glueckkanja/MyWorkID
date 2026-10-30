using System.Globalization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using NSubstitute;

namespace MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    /// <summary>
    /// Helpers for integration tests of the optional "require recent Verified ID" feature.
    /// </summary>
    internal static class VerifiedIdRequirementTestHelper
    {
        internal const string AttributeSetName = "MyWorkId";
        internal const string AttributeName = "lastVerifiedFaceCheck";

        /// <summary>
        /// Enables the Verified ID requirement with the given validity and configures the target security attribute.
        /// </summary>
        internal static void AddVerifiedIdRequirementConfig(
            this TestApplicationFactory factory,
            bool enabled = true,
            int? validityInMinutes = null,
            bool configureTargetSecurityAttribute = true)
        {
            var settings = new Dictionary<string, string?>
            {
                ["VerifiedIdRequirement:Enabled"] = enabled.ToString().ToLowerInvariant(),
            };
            if (validityInMinutes.HasValue)
            {
                settings["VerifiedIdRequirement:ValidityInMinutes"] = validityInMinutes.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (configureTargetSecurityAttribute)
            {
                settings["VerifiedId:TargetSecurityAttributeSet"] = AttributeSetName;
                settings["VerifiedId:TargetSecurityAttribute"] = AttributeName;
            }
            factory.ConfigureConfiguration(configurationBuilder => configurationBuilder.AddInMemoryCollection(settings));
        }

        /// <summary>
        /// Creates a Graph <see cref="User"/> whose custom security attribute holds the given verification timestamp,
        /// shaped like the Kiota deserializer returns it.
        /// </summary>
        internal static User CreateUserWithVerification(DateTimeOffset lastVerifiedAt)
        {
            return new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        [AttributeSetName] = new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            ["@odata.type"] = new UntypedString("#microsoft.graph.customSecurityAttributeValue"),
                            [AttributeName] = new UntypedString(Strings.VERIFIED_ID_BOUND_TIMESTAMP_PREFIX + lastVerifiedAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)),
                        }),
                    },
                },
            };
        }

        /// <summary>
        /// Creates a request adapter whose <c>SendAsync&lt;User&gt;</c> returns the given user for every request.
        /// </summary>
        internal static IRequestAdapter CreateRequestAdapterReturningUser(User? user)
        {
            var requestAdapter = Substitute.For<IRequestAdapter>();
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<User>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(user);
            return requestAdapter;
        }

        /// <summary>
        /// Creates a request adapter that returns a user verified <paramref name="minutesAgo"/> minutes ago.
        /// </summary>
        internal static IRequestAdapter CreateRequestAdapterForUserVerifiedMinutesAgo(int minutesAgo)
        {
            return CreateRequestAdapterReturningUser(CreateUserWithVerification(DateTimeOffset.UtcNow.AddMinutes(-minutesAgo)));
        }

        /// <summary>
        /// Replaces the <see cref="TimeProvider"/> of the application so the validity window is evaluated against a fixed clock.
        /// </summary>
        internal static WebApplicationFactory<T> WithTimeProvider<T>(
            this WebApplicationFactory<T> factory,
            TimeProvider timeProvider) where T : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => services.AddSingleton(timeProvider));
            });
        }

        /// <summary>
        /// Returns every Graph request that was sent through the given substituted request adapter, in call order.
        /// </summary>
        internal static List<RequestInformation> GetSentGraphRequests(IRequestAdapter requestAdapter)
        {
            return requestAdapter
                .ReceivedCalls()
                .SelectMany(call => call.GetArguments())
                .OfType<RequestInformation>()
                .ToList();
        }

        /// <summary>
        /// <c>true</c> if the request is the lookup of the user's custom security attributes performed by the Verified ID gate.
        /// </summary>
        internal static bool IsVerificationLookup(RequestInformation requestInformation)
        {
            return requestInformation.HttpMethod == Method.GET
                && requestInformation.URI.AbsolutePath.Contains("/users/", StringComparison.OrdinalIgnoreCase)
                && requestInformation.URI.Query.Contains("customSecurityAttributes", StringComparison.OrdinalIgnoreCase);
        }
    }
}
