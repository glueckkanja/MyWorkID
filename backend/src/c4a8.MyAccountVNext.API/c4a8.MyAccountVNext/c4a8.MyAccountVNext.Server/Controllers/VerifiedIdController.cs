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

        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<ActionResult> CreatePresentationRequest()
        {
            //Request.Body.Position = 0;

            using StreamReader streamReader = new StreamReader(Request.Body);
            var callbackBody = await streamReader.ReadToEndAsync();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<ActionResult> VerifyUser()
        {
            var response = await _verifiedIdService.CreatePresentationRequest();
            return Ok(response);
        }
    }
}
