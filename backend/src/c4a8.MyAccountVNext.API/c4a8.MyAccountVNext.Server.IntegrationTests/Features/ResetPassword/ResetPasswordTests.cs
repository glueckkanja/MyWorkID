using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.Features.ResetPassword.Entities;
using c4a8.MyAccountVNext.Server.IntegrationTests.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Features.PasswordReset
{
    public class ResetPasswordTests : IClassFixture<TestApplicationFactory>
    {
        private readonly string _baseUrl = "/api/me/resetPassword";
        private readonly TestApplicationFactory _testApplicationFactory;
        private readonly AppFunctionsOptions _appFunctionsOptions;

        public ResetPasswordTests(TestApplicationFactory testApplicationFactory)
        {
            _testApplicationFactory = testApplicationFactory;
            var configuration = testApplicationFactory.Services.GetRequiredService<IConfiguration>();
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
            var provider = new TestClaimsProvider().WithDismissUserRiskRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsync(_baseUrl, null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ResetPassword_WithAuth_WithoutAuthContext_Returns401WithMessage()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole();
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await CheckResponseHelper.CheckForInsuffienctClaimsResponse(response);
        }

        [Fact]
        public async Task ResetPassword_WithAuthContext_ButNoUserId_Returns401()
        {
            var provider = new TestClaimsProvider().WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!);
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var pwRequest = new PasswordResetRequest();
            var response = await client.PutAsJsonAsync(_baseUrl, pwRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [MemberData(nameof(GetInvalidPasswordInputsAndProblemDetailsErrorValidator))]
        public async Task ResetPassword_WithInvalidPassword_Returns400(PasswordResetRequest request, Action<KeyValuePair<string, string[]>> validator)
        {
            var provider = new TestClaimsProvider().WithRandomUserId().WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!);
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsJsonAsync(_baseUrl, request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Collection(problemDetails!.Errors, validator);
        }

        public static IEnumerable<object[]> GetInvalidPasswordInputsAndProblemDetailsErrorValidator()
        {
            var testData = new List<object[]>
            {
                new object[]
                {
                    new PasswordResetRequest(),
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_MISSING_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "skj" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_LENGTH_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "password" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "passwordA" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "password0" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "password#" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
            };

            return testData;
        }

        [Theory]
        [MemberData(nameof(GetValidPasswordInputsAndProblemDetailsErrorValidator))]
        public async Task ResetPassword_WithValidPasswordButInvaliUserId_Returns200(PasswordResetRequest request, Action<KeyValuePair<string, string[]>> validator)
        {
            var provider = new TestClaimsProvider().WithRandomUserId().WithResetPasswordRole().WithAuthContext(_appFunctionsOptions.ResetPassword!);
            var client = _testApplicationFactory.CreateClientWithTestAuth(provider);
            var response = await client.PutAsJsonAsync(_baseUrl, request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        public static IEnumerable<object[]> GetValidPasswordInputsAndProblemDetailsErrorValidator()
        {
            var testData = new List<object[]>
            {
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "passwordA0" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "passwordA#" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
                new object[]
                {
                    new PasswordResetRequest { NewPassword = "PASSWORD#1" },
                    new Action<KeyValuePair<string, string[]>>(kvp =>
                    {
                        kvp.Key.Should().Be(nameof(PasswordResetRequest.NewPassword));
                        kvp.Value.Should().ContainSingle().Which.Should().Be(Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR);
                    })
                },
            };

            return testData;
        }
    }
}
