﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Globalization;
using System.Net;
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
        public DismissUserRiskController(GraphServiceClient graphClient)
        {
            _graphServiceClient = graphClient;
        }

        [HttpPut]
        public IActionResult Get()
        {
            string claimsChallenge = CheckForRequiredAuthContext(Request.Method);
            if (string.IsNullOrWhiteSpace(claimsChallenge))
            {
                var userId = User.GetObjectId();
                if(userId == null)
                {
                    return StatusCode(StatusCodes.Status412PreconditionFailed, "UserId not provided");
                }
                _graphServiceClient.IdentityProtection.RiskyUsers.Dismiss.PostAsync(new Microsoft.Graph.IdentityProtection.RiskyUsers.Dismiss.DismissPostRequestBody() { UserIds = new List<string> { userId } });
                return Ok();
            }
            string missingAuthContextId = "c1";
            var base64str = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"access_token\":{\"acrs\":{\"essential\":true,\"value\":\"" + missingAuthContextId + "\"}}}"));
            var context = HttpContext;
            context.Response.Headers.Append("WWW-Authenticate", $"Bearer realm=\"\", authorization_uri=\"https://login.microsoftonline.com/common/oauth2/authorize\", error=\"insufficient_claims\", claims=\"" + base64str + "\"");
            context.Response.Headers.Append("Access-Control-Expose-Headers", "WWW-Authenticate");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            string message = string.Format(CultureInfo.InvariantCulture, "The presented access tokens had insufficient claims. Please request for claims requested in the WWW-Authentication header and try again.");
            context.Response.WriteAsync(message);
            context.Response.CompleteAsync();
            //throw new UnauthorizedAccessException(message);
            return StatusCode(401);
        }

        private string CheckForRequiredAuthContext(string method)
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
