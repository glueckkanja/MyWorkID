using System.Security.Claims;

namespace c4a8.MyAccountVNext.Server.IntegrationTests.Authentication
{
    public class TestClaimsProvider
    {
        public IList<Claim> Claims { get; }

        public TestClaimsProvider(IList<Claim> claims)
        {
            Claims = claims;
        }

        public TestClaimsProvider()
        {
            Claims = new List<Claim>();
        }

        public TestClaimsProvider WithRandomUserId()
        {
            var userId = Guid.NewGuid().ToString();
            Claims.Add(new Claim("sub", userId));
            Claims.Add(new Claim("oid", userId));
            return this;
        }

        public TestClaimsProvider AddRole(string role)
        {
            Claims.Add(new Claim(ClaimTypes.Role, role));
            return this;
        }

        public TestClaimsProvider WithGenerateTapRole()
        {
            Claims.Add(new Claim(ClaimTypes.Role, Strings.CREATE_TAP_ROLE));
            return this;
        }

        public TestClaimsProvider WithResetPasswordRole()
        {
            Claims.Add(new Claim(ClaimTypes.Role, Strings.RESET_PASSWORD_ROLE));
            return this;
        }

        public TestClaimsProvider WithDismissUserRiskRole()
        {
            Claims.Add(new Claim(ClaimTypes.Role, Strings.DISMISS_USER_RISK_ROLE));
            return this;
        }

        public TestClaimsProvider WithAuthContext(string authContextId)
        {
            Claims.Add(new Claim("acrs", authContextId));
            return this;
        }
    }
}
