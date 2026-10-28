using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using MyWorkID.Server.Exceptions;
using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Options;
using MyWorkID.Server.UnitTests.TestModels;
using NSubstitute;

namespace MyWorkID.Server.UnitTests.Features.VerifiedId
{
    public class VerifiedIdRequirementServiceTests
    {
        private const string AttributeSetName = "MyWorkID";
        private const string AttributeName = "lastVerifiedFaceCheck";
        private static readonly DateTimeOffset _now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public void IsEnabledAndValidity_ReflectOptions()
        {
            var sut = CreateService(enabled: true, validityInMinutes: 120, requestAdapter: Substitute.For<IRequestAdapter>());

            sut.IsEnabled.Should().BeTrue();
            sut.Validity.Should().Be(TimeSpan.FromMinutes(120));
        }

        [Fact]
        public void Evaluate_WithVerificationInsideWindow_IsValid()
        {
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter: Substitute.For<IRequestAdapter>());
            DateTimeOffset lastVerifiedAt = _now.AddMinutes(-30);

            VerifiedIdVerificationState state = sut.Evaluate(lastVerifiedAt);

            state.IsValid.Should().BeTrue();
            state.LastVerifiedAt.Should().Be(lastVerifiedAt);
            state.ValidUntil.Should().Be(lastVerifiedAt.AddMinutes(60));
        }

        [Fact]
        public void Evaluate_WithVerificationExactlyAtExpiry_IsInvalid()
        {
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter: Substitute.For<IRequestAdapter>());

            VerifiedIdVerificationState state = sut.Evaluate(_now.AddMinutes(-60));

            state.IsValid.Should().BeFalse();
            state.ValidUntil.Should().Be(_now);
        }

        [Fact]
        public void Evaluate_WithExpiredVerification_IsInvalidButKeepsTimestamps()
        {
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter: Substitute.For<IRequestAdapter>());
            DateTimeOffset lastVerifiedAt = _now.AddHours(-5);

            VerifiedIdVerificationState state = sut.Evaluate(lastVerifiedAt);

            state.IsValid.Should().BeFalse();
            state.LastVerifiedAt.Should().Be(lastVerifiedAt);
            state.ValidUntil.Should().Be(lastVerifiedAt.AddMinutes(60));
        }

        [Fact]
        public void Evaluate_WithVerificationInTheFuture_IsInvalid()
        {
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter: Substitute.For<IRequestAdapter>());

            VerifiedIdVerificationState state = sut.Evaluate(_now.AddMinutes(10));

            state.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(0, true)] // verified exactly now: the window is inclusive at its start
        [InlineData(-1, true)] // verified one tick ago
        [InlineData(1, false)] // one tick in the future
        public void Evaluate_AtStartOfValidityWindow_HasTickPrecision(long offsetInTicks, bool expectedIsValid)
        {
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter: Substitute.For<IRequestAdapter>());

            VerifiedIdVerificationState state = sut.Evaluate(_now.AddTicks(offsetInTicks));

            state.IsValid.Should().Be(expectedIsValid);
        }

        [Theory]
        [InlineData(1, true)] // expires one tick from now
        [InlineData(0, false)] // expires exactly now: the window is exclusive at its end
        [InlineData(-1, false)] // expired one tick ago
        public void Evaluate_AtEndOfValidityWindow_HasTickPrecision(long ticksUntilExpiry, bool expectedIsValid)
        {
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter: Substitute.For<IRequestAdapter>());
            DateTimeOffset lastVerifiedAt = _now.AddMinutes(-60).AddTicks(ticksUntilExpiry);

            VerifiedIdVerificationState state = sut.Evaluate(lastVerifiedAt);

            state.IsValid.Should().Be(expectedIsValid);
            state.ValidUntil.Should().Be(_now.AddTicks(ticksUntilExpiry));
        }

        [Fact]
        public async Task GetVerificationStateAsync_WithFutureTimestampInGraph_ReturnsInvalidStateWithTimestamp()
        {
            DateTimeOffset lastVerifiedAt = _now.AddMinutes(5);
            IRequestAdapter requestAdapter = CreateRequestAdapterReturning(CreateUser(lastVerifiedAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)));
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter);

            VerifiedIdVerificationState state = await sut.GetVerificationStateAsync(Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

            state.IsValid.Should().BeFalse();
            state.LastVerifiedAt.Should().Be(lastVerifiedAt);
            state.ValidUntil.Should().Be(lastVerifiedAt.AddMinutes(60));
        }

        [Fact]
        public async Task GetVerificationStateAsync_WithRecentTimestampInGraph_ReturnsValidState()
        {
            DateTimeOffset lastVerifiedAt = _now.AddMinutes(-10);
            IRequestAdapter requestAdapter = CreateRequestAdapterReturning(CreateUser(lastVerifiedAt.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)));
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter);

            VerifiedIdVerificationState state = await sut.GetVerificationStateAsync(Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

            state.IsValid.Should().BeTrue();
            state.LastVerifiedAt.Should().Be(lastVerifiedAt);
            state.ValidUntil.Should().Be(lastVerifiedAt.AddMinutes(60));
        }

        [Fact]
        public async Task GetVerificationStateAsync_RequestsCustomSecurityAttributesForUser()
        {
            string userId = Guid.NewGuid().ToString();
            IRequestAdapter requestAdapter = CreateRequestAdapterReturning(CreateUser(_now.UtcDateTime.ToString("O", CultureInfo.InvariantCulture)));
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter);

            await sut.GetVerificationStateAsync(userId, TestContext.Current.CancellationToken);

            await requestAdapter.Received(1).SendAsync(
                Arg.Is<RequestInformation>(requestInformation =>
                    requestInformation.HttpMethod == Method.GET
                    && requestInformation.PathParameters.Values.Contains(userId)
                    && requestInformation.URI.Query.Contains("customSecurityAttributes")),
                Arg.Any<ParsableFactory<User>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetVerificationStateAsync_WithoutTimestampInGraph_ReturnsNeverVerified()
        {
            IRequestAdapter requestAdapter = CreateRequestAdapterReturning(new User());
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter);

            VerifiedIdVerificationState state = await sut.GetVerificationStateAsync(Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

            state.Should().Be(VerifiedIdVerificationState.NeverVerified);
            state.IsValid.Should().BeFalse();
            state.LastVerifiedAt.Should().BeNull();
            state.ValidUntil.Should().BeNull();
        }

        [Fact]
        public async Task GetVerificationStateAsync_WhenGraphReturnsNull_ReturnsNeverVerified()
        {
            IRequestAdapter requestAdapter = CreateRequestAdapterReturning(null);
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter);

            VerifiedIdVerificationState state = await sut.GetVerificationStateAsync(Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

            state.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(null, AttributeName, "VerifiedId:TargetSecurityAttributeSet")]
        [InlineData("", AttributeName, "VerifiedId:TargetSecurityAttributeSet")]
        [InlineData(AttributeSetName, null, "VerifiedId:TargetSecurityAttribute")]
        [InlineData(AttributeSetName, " ", "VerifiedId:TargetSecurityAttribute")]
        public async Task GetVerificationStateAsync_WithoutTargetAttributeConfiguration_Throws(string? attributeSetName, string? attributeName, string expectedConfigurationKey)
        {
            IRequestAdapter requestAdapter = Substitute.For<IRequestAdapter>();
            var sut = CreateService(enabled: true, validityInMinutes: 60, requestAdapter, attributeSetName, attributeName);

            Func<Task> act = () => sut.GetVerificationStateAsync(Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

            var exception = await act.Should().ThrowAsync<ConfigurationMissingException>();
            exception.Which.ConfigurationKey.Should().Be(expectedConfigurationKey);
            await requestAdapter.DidNotReceiveWithAnyArgs().SendAsync(
                default!,
                Arg.Any<ParsableFactory<User>>(),
                default,
                TestContext.Current.CancellationToken);
        }

        private static VerifiedIdRequirementService CreateService(
            bool enabled,
            int validityInMinutes,
            IRequestAdapter requestAdapter,
            string? attributeSetName = AttributeSetName,
            string? attributeName = AttributeName)
        {
            var requirementOptions = Microsoft.Extensions.Options.Options.Create(new VerifiedIdRequirementOptions
            {
                Enabled = enabled,
                ValidityInMinutes = validityInMinutes,
            });
            var verifiedIdOptions = Microsoft.Extensions.Options.Options.Create(new VerifiedIdOptions
            {
                TargetSecurityAttributeSet = attributeSetName,
                TargetSecurityAttribute = attributeName,
            });
            var graphClient = new GraphServiceClient(requestAdapter);
            var logger = Substitute.For<ILogger<VerifiedIdRequirementService>>();
            return new VerifiedIdRequirementService(requirementOptions, verifiedIdOptions, graphClient, new FixedTimeProvider(_now), logger);
        }

        private static IRequestAdapter CreateRequestAdapterReturning(User? user)
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

        private static User CreateUser(string timestamp)
        {
            return new User
            {
                CustomSecurityAttributes = new CustomSecurityAttributeValue
                {
                    AdditionalData = new Dictionary<string, object>
                    {
                        [AttributeSetName] = new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            [AttributeName] = new UntypedString(timestamp),
                        }),
                    },
                },
            };
        }
    }
}
