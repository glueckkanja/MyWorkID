using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.ResetPassword.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.PasswordReset
{
    public class ResetPasswordTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/resetPassword";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly string _validAuthContextId;
        private readonly TestApplicationFactory _configuredTestApplicationFactory;

        public ResetPasswordTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuredTestApplicationFactory = new TestApplicationFactory();
            _validAuthContextId = $"c{new Random().Next(1, 100)}";
            configuredTestApplicationFactory.AddAuthContextConfig(AppFunctions.ResetPassword.ToString(), _validAuthContextId);
            _configuredTestApplicationFactory = configuredTestApplicationFactory;
        }

        [Fact]
        public async Task ResetPassword_WithoutAuth_Returns401()
        {
            var unauthenticatedClient = _testApplicationFactory.CreateDefaultClient();
            var response = await unauthenticatedClient.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ResetPassword_WithWrongRole_Returns403()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithDismissUserRiskRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ResetPassword_WithoutAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithResetPasswordRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_RESET_PASSWORD);
        }

        // Tests if the set auth conext id is valid (c1-c99)
        [Fact]
        public async Task ResetPassword_WithAuth_WithIncorrectAppSetting_Returns500WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.ResetPassword.ToString(), "invalid");
            var client = TestHelper.CreateClientWithRole(testApp, provider => provider.WithResetPasswordRole());
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Detail.Should().Be(Strings.ERROR_MISSING_OR_INVALID_SETTINGS_RESET_PASSWORD);
        }

        [Fact]
        public async Task ResetPassword_WithAuth_WithIncorrectAuthContext_Returns401WithMessage()
        {
            var testApp = new TestApplicationFactory();
            testApp.AddAuthContextConfig(AppFunctions.ResetPassword.ToString(), "c1");
            var client = TestHelper.CreateClientWithRole(testApp,
                provider => provider.WithResetPasswordRole().WithAuthContext("c2"));
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task ResetPassword_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory, provider => provider.WithResetPasswordRole());
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task ResetPassword_WithAuthContext_ButNoUserId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithResetPasswordRole().WithAuthContext(_validAuthContextId));
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [MemberData(nameof(GetInvalidPasswordInputsAndProblemDetailsErrorValidator))]
        public async Task ResetPassword_WithInvalidPassword_Returns400(PasswordResetRequest request, Action<KeyValuePair<string, string[]>> validator)
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithRandomSubAndOid().WithResetPasswordRole().WithAuthContext(_validAuthContextId));
            var response = await client.PutAsJsonAsync(_baseUrl, request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            var error = Assert.Single(problemDetails!.Errors);
            validator(error);
        }

        public static TheoryData<PasswordResetRequest, Action<KeyValuePair<string, string[]>>> GetInvalidPasswordInputsAndProblemDetailsErrorValidator()
        {
            return new TheoryData<PasswordResetRequest, Action<KeyValuePair<string, string[]>>>
            {
                {
                    new PasswordResetRequest(),
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_MISSING_ERROR);
                    })
                },
                {
                    new PasswordResetRequest { NewPassword = "skj" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_LENGTH_ERROR);
                    })
                },
                {
                    new PasswordResetRequest { NewPassword = "password" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                {
                    new PasswordResetRequest { NewPassword = "passwordA" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                {
                    new PasswordResetRequest { NewPassword = "password0" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                {
                    new PasswordResetRequest { NewPassword = "password#" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
            };
        }

        [Theory]
        [MemberData(nameof(GetValidPasswordInputs))]
        public async Task ResetPassword_WithValidPasswords_Returns200(PasswordResetRequest request)
        {
            var client = TestHelper.CreateClientWithRole(_configuredTestApplicationFactory,
                provider => provider.WithRandomSubAndOid().WithResetPasswordRole().WithAuthContext(_validAuthContextId));
            var response = await client.PutAsJsonAsync(_baseUrl, request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public static TheoryData<PasswordResetRequest> GetValidPasswordInputs()
        {
            return new TheoryData<PasswordResetRequest>
            {
                    new PasswordResetRequest { NewPassword = "passwordA0" },
                    new PasswordResetRequest { NewPassword = "passwordA#" },
                    new PasswordResetRequest { NewPassword = "PASSWORD#1" }
            };
        }
    }
}
