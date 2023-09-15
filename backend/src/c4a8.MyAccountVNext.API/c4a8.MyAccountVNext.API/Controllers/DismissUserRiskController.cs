using c4a8.MyAccountVNext.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "MyAccount.VNext.DismissUserRisk")]
    public class DismissUserRiskController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IAuthContextService _authContextService;

        public DismissUserRiskController(GraphServiceClient graphClient, IAuthContextService authContextService)
        {
            _graphServiceClient = graphClient;
            _authContextService = authContextService;
        }

        [HttpPut]
        public IActionResult Get()
        {
            string? missingAuthContextId = _authContextService.GetAuthContextId(AppFunctions.DismissUserRisk);
            string claimsChallenge = CheckForRequiredAuthContext(missingAuthContextId);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = User.GetObjectId();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, "UserId not provided");
                }
                _graphServiceClient.IdentityProtection.RiskyUsers.Dismiss.PostAsync(new Microsoft.Graph.IdentityProtection.RiskyUsers.Dismiss.DismissPostRequestBody() { UserIds = new List<string> { userId } });
                return Ok();
            }
            var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + missingAuthContextId + "\"}}}"));
            var context = HttpContext;
            context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", error=\"insufficient_claims\", claims=\"" + base64str + "\"");
            context.Response.Headers.Append("Access-Control-Expose-Headers", "WWW-Authenticate");
            string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
            context.Response.WriteAsync(message);
            context.Response.CompleteAsync();
            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        private string CheckForRequiredAuthContext(string? authContextId)
        {
            string claimsChallenge = string.Empty;

            if (!string.IsNullOrEmpty(authContextId))
            {
                HttpContext context = this.HttpContext;

                string authenticationContextClassReferencesClaim = "acrs";

                if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
                {
                    throw new ArgumentNullException("No Usercontext is available to pick claims from");
                }

                Claim acrsClaim = context.User.FindAll(authenticationContextClassReferencesClaim).FirstOrDefault(x => x.Value == authContextId);

                if (acrsClaim?.Value != authContextId)
                {
                    claimsChallenge = "{\"id_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + authContextId + "\"}}}";

                }
            }

            return claimsChallenge;
        }
    }
}
