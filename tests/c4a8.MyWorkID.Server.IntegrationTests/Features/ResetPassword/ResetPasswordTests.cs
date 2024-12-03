using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.ResetPassword.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyWorkID.Server.IntegrationTests.Features.PasswordReset
{
    public class ResetPasswordTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/resetPassword";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public ResetPasswordTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            _appFunctionsOptions = testApplicationFactory.Services.GetRequiredService<IOptions<AppFunctionsOptions>>().Value;
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
        public async Task ResetPassword_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory, provider => provider.WithResetPasswordRole());
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task ResetPassword_WithAuthContext_ButNoUserId_Returns401()
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!));
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [MemberData(nameof(GetInvalidPasswordInputsAndProblemDetailsErrorValidator))]
        public async Task ResetPassword_WithInvalidPassword_Returns400(PasswordResetRequest request, Action<KeyValuePair<string, string[]>> validator)
        {
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithRandomSubAndOid().WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!));
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
            var client = TestHelper.CreateClientWithRole(_testApplicationFactory,
                provider => provider.WithRandomSubAndOid().WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!));
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
