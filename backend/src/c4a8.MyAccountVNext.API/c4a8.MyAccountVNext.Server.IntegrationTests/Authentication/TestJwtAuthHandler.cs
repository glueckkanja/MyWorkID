using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Authentication
{
    public class TestJwtAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string TestScheme = "TestJwtBearer";

        public TestJwtAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, TestScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, TestScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}