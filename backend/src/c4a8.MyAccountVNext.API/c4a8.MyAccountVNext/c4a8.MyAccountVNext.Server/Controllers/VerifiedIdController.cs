using c4a8.MyAccountVNext.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace c4a8.MyAccountVNext.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifiedIdController : ControllerBase
    {
        private readonly VerifiedIdService _verifiedIdService;
        public VerifiedIdController(VerifiedIdService verifiedIdService)
        {
            _verifiedIdService = verifiedIdService;
        }

        [Authorize]
        [HttpPost("callback")]
        public async Task<ActionResult> CreatePresentationRequest()
        {
            using StreamReader streamReader = new StreamReader(Request.Body);
            var callbackBody = await streamReader.ReadToEndAsync();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [Authorize(Roles = "MyAccount.VNext.VerifiedId.Verify")]
        [HttpPost("verify")]
        public async Task<ActionResult> VerifyUser()
        {
            var response = await _verifiedIdService.CreatePresentationRequest();
            return Ok(response);
        }
    }
}
