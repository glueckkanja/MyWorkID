using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "MyAccount.VNext.DismissUserRisk")]
    public class DismissUserRiskController : ControllerBase
    {
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public DismissUserRiskController(MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _consentHandler = consentHandler;
        }

        [HttpPut]
        public IActionResult Get()
        {
            string claimsChallenge = CheckForRequiredAuthContext(Request.Method);
            if (!string.IsNullOrWhiteSpace(claimsChallenge))
            {
                _consentHandler.ChallengeUser(new string[] { "MyAccount.VNext.DismissUserRisk" }, claimsChallenge);

                return new EmptyResult();
            }

            return new StatusCodeResult(StatusCodes.Status200OK);
        }

        public string CheckForRequiredAuthContext(string method)
        {
            string claimsChallenge = string.Empty;

            string savedAuthContextId = "c1";

            if (!string.IsNullOrEmpty(savedAuthContextId))
            {
                HttpContext context = this.HttpContext;

                string authenticationContextClassReferencesClaim = "acrs";

                if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
                {
                    throw new ArgumentNullException("No Usercontext is available to pick claims from");
                }

                Claim acrsClaim = context.User.FindAll(authenticationContextClassReferencesClaim).FirstOrDefault(x => x.Value == savedAuthContextId);

                if (acrsClaim?.Value != savedAuthContextId)
                {
                    claimsChallenge = "{\"id_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + savedAuthContextId + "\"}}}";

                }
            }

            return claimsChallenge;
        }
    }
}
