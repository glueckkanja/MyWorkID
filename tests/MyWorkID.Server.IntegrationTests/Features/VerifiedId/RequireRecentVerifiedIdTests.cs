using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using MyWorkID.Server.Common;
using MyWorkID.Server.Features.ResetPassword.Entities;
using MyWorkID.Server.IntegrationTests.Authentication;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyWorkID.Server.IntegrationTests.Features.VerifiedId
{
    /// <summary>
    /// Verifies that the optional "require recent Verified ID" gate protects every sensitive endpoint
    /// (generate TAP, reset password, dismiss user risk) and stays inactive when disabled.
    /// All tests run against a fixed clock so the validity window boundaries are deterministic, and they
    /// inspect the Graph requests that were sent to prove whether the gated action was actually invoked.
    /// </summary>
    public class RequireRecentVerifiedIdTests
    {
        private const string _validAuthContextId = "c5";
        private const int _validityInMinutes = 60;
        private static readonly DateTimeOffset _now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        private static readonly TimeSpan _validity = TimeSpan.FromMinutes(_validityInMinutes);
        private static readonly PasswordResetRequest _validPasswordResetRequest = new() { NewPassword = "Str0ng!Passw0rd" };

        public enum GatedAction
        {
            GenerateTemporaryAccessPass,
            ResetPassword,
            DismissUserRisk,
        }

        public static TheoryData<GatedAction> GatedActions => new()
        {
            GatedAction.GenerateTemporaryAccessPass,
            GatedAction.ResetPassword,
            GatedAction.DismissUserRisk,
        };

        /// <summary>
        /// Verification timestamps (as offset in seconds from the fixed "now") that are inside the validity window.
        /// </summary>
        private static readonly int[] _validVerificationOffsetsInSeconds =
        [
            0, // verified right now
            -1, // verified one second ago
            -(int)_validity.TotalSeconds + 1, // one second before the verification expires
        ];

        /// <summary>
        /// Verification timestamps (as offset in seconds from the fixed "now") that must be rejected.
        /// </summary>
        private static readonly int[] _rejectedVerificationOffsetsInSeconds =
        [
            -(int)_validity.TotalSeconds, // exactly at expiry: the window is exclusive at its end
            -(int)_validity.TotalSeconds - 1, // one second after expiry
            -(int)TimeSpan.FromDays(30).TotalSeconds, // long expired
            1, // one second in the future (clock skew or tampered attribute)
            (int)TimeSpan.FromDays(1).TotalSeconds, // far in the future
        ];

        public static TheoryData<GatedAction, int> GatedActionsWithValidVerification =>
            CrossJoin(_validVerificationOffsetsInSeconds);

        public static TheoryData<GatedAction, int> GatedActionsWithRejectedVerification =>
            CrossJoin(_rejectedVerificationOffsetsInSeconds);

        [Theory]
        [MemberData(nameof(GatedActions))]
        public async Task GatedEndpoint_WhenRequirementEnabledAndUserNeverVerified_Returns403WithoutInvokingAction(GatedAction gatedAction)
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(new User());
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: true);

            var response = await SendAsync(client, gatedAction);

            ProblemDetails problemDetails = await AssertBlockedByVerifiedIdGateAsync(response, requestAdapter);
            problemDetails.Detail.Should().Be(Strings.ERROR_VERIFIED_ID_VERIFICATION_REQUIRED);
            problemDetails.Extensions.Should().Contain("lastVerifiedAt", null);
            problemDetails.Extensions.Should().Contain("validUntil", null);
        }

        [Theory]
        [MemberData(nameof(GatedActions))]
        public async Task GatedEndpoint_WhenRequirementEnabledAndGraphReturnsNoUser_Returns403WithoutInvokingAction(GatedAction gatedAction)
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(null);
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: true);

            var response = await SendAsync(client, gatedAction);

            ProblemDetails problemDetails = await AssertBlockedByVerifiedIdGateAsync(response, requestAdapter);
            problemDetails.Detail.Should().Be(Strings.ERROR_VERIFIED_ID_VERIFICATION_REQUIRED);
        }

        [Theory]
        [MemberData(nameof(GatedActions))]
        public async Task GatedEndpoint_WhenRequirementEnabledAndVerificationExpired_Returns403WithExpiredMessageWithoutInvokingAction(GatedAction gatedAction)
        {
            DateTimeOffset lastVerifiedAt = _now - _validity - TimeSpan.FromMinutes(1);
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(lastVerifiedAt));
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: true);

            var response = await SendAsync(client, gatedAction);

            ProblemDetails problemDetails = await AssertBlockedByVerifiedIdGateAsync(response, requestAdapter);
            problemDetails.Detail.Should().Be(Strings.ERROR_VERIFIED_ID_VERIFICATION_EXPIRED);
            GetExtension(problemDetails, "lastVerifiedAt").GetDateTimeOffset().Should().Be(lastVerifiedAt);
            GetExtension(problemDetails, "validUntil").GetDateTimeOffset().Should().Be(lastVerifiedAt + _validity);
        }

        [Theory]
        [MemberData(nameof(GatedActionsWithRejectedVerification))]
        public async Task GatedEndpoint_WhenVerificationIsAtOrBeyondExpiryOrInTheFuture_Returns403WithoutInvokingAction(GatedAction gatedAction, int verificationOffsetInSeconds)
        {
            DateTimeOffset lastVerifiedAt = _now.AddSeconds(verificationOffsetInSeconds);
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(lastVerifiedAt));
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: true);

            var response = await SendAsync(client, gatedAction);

            ProblemDetails problemDetails = await AssertBlockedByVerifiedIdGateAsync(response, requestAdapter);
            GetExtension(problemDetails, "lastVerifiedAt").GetDateTimeOffset().Should().Be(lastVerifiedAt);
            GetExtension(problemDetails, "validUntil").GetDateTimeOffset().Should().Be(lastVerifiedAt + _validity);
        }

        [Theory]
        [MemberData(nameof(GatedActionsWithValidVerification))]
        public async Task GatedEndpoint_WhenVerificationIsInsideValidityWindow_InvokesActionAndReturns200(GatedAction gatedAction, int verificationOffsetInSeconds)
        {
            DateTimeOffset lastVerifiedAt = _now.AddSeconds(verificationOffsetInSeconds);
            IRequestAdapter requestAdapter = CreateRequestAdapter(VerifiedIdRequirementTestHelper.CreateUserWithVerification(lastVerifiedAt));
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: true);

            var response = await SendAsync(client, gatedAction);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            List<RequestInformation> sentGraphRequests = VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter);
            sentGraphRequests.Should().Contain(request => VerifiedIdRequirementTestHelper.IsVerificationLookup(request));
            sentGraphRequests.Where(request => IsActionRequest(gatedAction, request)).Should().ContainSingle();
        }

        [Theory]
        [MemberData(nameof(GatedActions))]
        public async Task GatedEndpoint_WhenRequirementDisabled_InvokesActionWithoutLookingUpVerification(GatedAction gatedAction)
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(new User());
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: false);

            var response = await SendAsync(client, gatedAction);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            List<RequestInformation> sentGraphRequests = VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter);
            sentGraphRequests.Should().NotContain(request => VerifiedIdRequirementTestHelper.IsVerificationLookup(request));
            sentGraphRequests.Where(request => IsActionRequest(gatedAction, request)).Should().ContainSingle();
        }

        [Theory]
        [MemberData(nameof(GatedActions))]
        public async Task GatedEndpoint_WhenRequirementEnabledWithoutTargetAttributeConfig_Returns500WithoutInvokingAction(GatedAction gatedAction)
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(new User());
            var client = CreateClient(gatedAction, requestAdapter, requirementEnabled: true, configureTargetSecurityAttribute: false);

            var response = await SendAsync(client, gatedAction);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_VERIFIED_ID_REQUIREMENT);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter).Should().BeEmpty();
        }

        [Fact]
        public async Task RevokeTemporaryAccessPass_IsNotGated_WhenRequirementEnabled()
        {
            IRequestAdapter requestAdapter = CreateRequestAdapter(new User());
            var client = CreateClient(GatedAction.GenerateTemporaryAccessPass, requestAdapter, requirementEnabled: true);

            var response = await client.DeleteAsync($"/api/me/tap/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter)
                .Should().NotContain(request => VerifiedIdRequirementTestHelper.IsVerificationLookup(request));
        }

        /// <summary>
        /// Asserts that the response is the 403 of the Verified ID gate and that the only Graph request that was sent
        /// is the verification lookup, i.e. the gated action itself was never invoked.
        /// </summary>
        private static async Task<ProblemDetails> AssertBlockedByVerifiedIdGateAsync(HttpResponseMessage response, IRequestAdapter requestAdapter)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
            problemDetails.Should().NotBeNull();
            GetExtension(problemDetails!, "verifiedIdRequired").GetBoolean().Should().BeTrue();

            VerifiedIdRequirementTestHelper.GetSentGraphRequests(requestAdapter)
                .Should().ContainSingle()
                .Which.Should().Match<RequestInformation>(request => VerifiedIdRequirementTestHelper.IsVerificationLookup(request));
            return problemDetails!;
        }

        private static JsonElement GetExtension(ProblemDetails problemDetails, string key)
        {
            problemDetails.Extensions.Should().ContainKey(key);
            return (JsonElement)problemDetails.Extensions[key]!;
        }

        private static bool IsActionRequest(GatedAction gatedAction, RequestInformation requestInformation)
        {
            string path = requestInformation.URI.AbsolutePath;
            return gatedAction switch
            {
                GatedAction.GenerateTemporaryAccessPass => requestInformation.HttpMethod == Method.POST
                    && path.EndsWith("/authentication/temporaryAccessPassMethods", StringComparison.OrdinalIgnoreCase),
                GatedAction.ResetPassword => requestInformation.HttpMethod == Method.PATCH
                    && path.Contains("/users/", StringComparison.OrdinalIgnoreCase),
                GatedAction.DismissUserRisk => requestInformation.HttpMethod == Method.POST
                    && path.EndsWith("/identityProtection/riskyUsers/dismiss", StringComparison.OrdinalIgnoreCase),
                _ => throw new ArgumentOutOfRangeException(nameof(gatedAction), gatedAction, null),
            };
        }

        private static Task<HttpResponseMessage> SendAsync(HttpClient client, GatedAction gatedAction)
        {
            return client.PutAsJsonAsync(GetUrl(gatedAction), _validPasswordResetRequest, TestContext.Current.CancellationToken);
        }

        private static string GetUrl(GatedAction gatedAction)
        {
            return gatedAction switch
            {
                GatedAction.GenerateTemporaryAccessPass => "/api/me/generatetap",
                GatedAction.ResetPassword => "/api/me/resetPassword",
                GatedAction.DismissUserRisk => "/api/me/riskstate/dismiss",
                _ => throw new ArgumentOutOfRangeException(nameof(gatedAction), gatedAction, null),
            };
        }

        private static (AppFunctions AppFunction, string Role) GetAppFunctionAndRole(GatedAction gatedAction)
        {
            return gatedAction switch
            {
                GatedAction.GenerateTemporaryAccessPass => (AppFunctions.GenerateTap, Strings.CREATE_TAP_ROLE),
                GatedAction.ResetPassword => (AppFunctions.ResetPassword, Strings.RESET_PASSWORD_ROLE),
                GatedAction.DismissUserRisk => (AppFunctions.DismissUserRisk, Strings.DISMISS_USER_RISK_ROLE),
                _ => throw new ArgumentOutOfRangeException(nameof(gatedAction), gatedAction, null),
            };
        }

        private static HttpClient CreateClient(
            GatedAction gatedAction,
            IRequestAdapter requestAdapter,
            bool requirementEnabled,
            bool configureTargetSecurityAttribute = true)
        {
            (AppFunctions appFunction, string role) = GetAppFunctionAndRole(gatedAction);
            var testApplicationFactory = new TestApplicationFactory();
            testApplicationFactory.AddAuthContextConfig(appFunction.ToString(), _validAuthContextId);
            testApplicationFactory.AddVerifiedIdRequirementConfig(requirementEnabled, _validityInMinutes, configureTargetSecurityAttribute);

            var claimsProvider = new TestClaimsProvider()
                .AddRole(role)
                .WithRandomSubAndOid()
                .WithAuthContext(_validAuthContextId);

            return testApplicationFactory
                .WithAuthentication(claimsProvider, requestAdapter)
                .WithTimeProvider(new FixedTimeProvider(_now))
                .CreateClient();
        }

        /// <summary>
        /// Creates a request adapter that returns <paramref name="user"/> for the verification lookup and lets
        /// every gated action succeed, so a 200 proves the request passed the Verified ID gate.
        /// </summary>
        private static IRequestAdapter CreateRequestAdapter(User? user)
        {
            IRequestAdapter requestAdapter = VerifiedIdRequirementTestHelper.CreateRequestAdapterReturningUser(user);
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<TemporaryAccessPassAuthenticationMethod>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(new TemporaryAccessPassAuthenticationMethod
                {
                    Id = Guid.NewGuid().ToString(),
                    TemporaryAccessPass = "TemporaryAccessPassValue",
                });
            requestAdapter.SendAsync(
                Arg.Any<RequestInformation>(),
                Arg.Any<ParsableFactory<RiskyUser>>(),
                Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
                Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(new RiskyUser { RiskState = RiskState.AtRisk, RiskLevel = RiskLevel.Low });
            return requestAdapter;
        }

        private static TheoryData<GatedAction, int> CrossJoin(IEnumerable<int> verificationOffsetsInSeconds)
        {
            var theoryData = new TheoryData<GatedAction, int>();
            foreach (GatedAction gatedAction in Enum.GetValues<GatedAction>())
            {
                foreach (int verificationOffsetInSeconds in verificationOffsetsInSeconds)
                {
                    theoryData.Add(gatedAction, verificationOffsetInSeconds);
                }
            }
            return theoryData;
        }
    }
}
