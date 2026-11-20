using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;
using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Features.VerifiedId.Entities;
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
        internal static User CreateUserWithVerification(DateTimeOffset lastVerifiedAt, VerifiedIdAction? verifiedForAction = null)
        {
            return CreateUserWithAttributeValue(VerificationAttributeValue.Create(lastVerifiedAt, verifiedForAction));
        }

        /// <summary>
        /// Creates a Graph <see cref="User"/> whose verification was already used for an action at the given time.
        /// </summary>
        internal static User CreateUserWithUsedVerification(DateTimeOffset usedAt)
        {
            return CreateUserWithAttributeValue(Strings.VERIFIED_ID_USED_TIMESTAMP_PREFIX + usedAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Creates a Graph <see cref="User"/> whose custom security attribute holds the given raw value.
        /// </summary>
        internal static User CreateUserWithAttributeValue(string attributeValue)
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
                            [AttributeName] = new UntypedString(attributeValue),
                        }),
                    },
                },
            };
        }

        /// <summary>
        /// Creates a request adapter whose <c>SendAsync&lt;User&gt;</c> returns the given user. Once a request
        /// overwrites the verification attribute, later requests return a user holding the written value, like Graph
        /// does. Writes are only visible with <see cref="WithJsonSerialization"/>, because they are read from the body.
        /// </summary>
        internal static IRequestAdapter CreateRequestAdapterReturningUser(User? user)
        {
            User? currentUser = user;
            var requestAdapter = Substitute.For<IRequestAdapter>();
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<User>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(callInfo =>
                {
                    string? writtenAttributeValue = GetWrittenAttributeValue(callInfo.ArgAt<RequestInformation>(0));
                    if (writtenAttributeValue is not null)
                    {
                        currentUser = CreateUserWithAttributeValue(writtenAttributeValue);
                    }
                    return currentUser;
                });
            return requestAdapter;
        }

        /// <summary>
        /// Returns the verification attribute value a request writes, or <c>null</c> if it does not write it.
        /// </summary>
        internal static string? GetWrittenAttributeValue(RequestInformation requestInformation)
        {
            if (requestInformation.HttpMethod != Method.PATCH)
            {
                return null;
            }

            string content = ReadContent(requestInformation);
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            using JsonDocument body = JsonDocument.Parse(content);
            if (body.RootElement.TryGetProperty("customSecurityAttributes", out JsonElement attributeSets)
                && attributeSets.TryGetProperty(AttributeSetName, out JsonElement attributeSet)
                && attributeSet.TryGetProperty(AttributeName, out JsonElement attributeValue))
            {
                return attributeValue.GetString();
            }
            return null;
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

        /// <summary>
        /// Serializes request bodies with the real Kiota JSON serializer, so tests can inspect what was sent to Graph.
        /// Without it, the substituted request adapter leaves every request body empty.
        /// </summary>
        internal static IRequestAdapter WithJsonSerialization(this IRequestAdapter requestAdapter)
        {
            requestAdapter.SerializationWriterFactory.Returns(new JsonSerializationWriterFactory());
            return requestAdapter;
        }

        /// <summary>
        /// <c>true</c> if the request overwrites the user's verification with the "used" marker. Requires
        /// <see cref="WithJsonSerialization"/>, because the marker is only visible in the request body.
        /// </summary>
        internal static bool IsVerificationMarkedAsUsed(RequestInformation requestInformation)
        {
            return requestInformation.HttpMethod == Method.PATCH
                && requestInformation.URI.AbsolutePath.Contains("/users/", StringComparison.OrdinalIgnoreCase)
                && ReadContent(requestInformation).Contains(Strings.VERIFIED_ID_USED_TIMESTAMP_PREFIX, StringComparison.Ordinal);
        }

        internal static string ReadContent(RequestInformation requestInformation)
        {
            if (requestInformation.Content is null)
            {
                return string.Empty;
            }

            requestInformation.Content.Position = 0;
            using var reader = new StreamReader(requestInformation.Content, leaveOpen: true);
            string content = reader.ReadToEnd();
            requestInformation.Content.Position = 0;
            return content;
        }
    }
}
