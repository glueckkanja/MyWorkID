using c4a8.MyAccountVNext.Server.Hubs;
using c4a8.MyAccountVNext.Server.Models.VerifiedId;
using c4a8.MyAccountVNext.Server.Repositories;
using c4a8.MyAccountVNext.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace c4a8.MyAccountVNext.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifiedIdController : ControllerBase
    {
        private readonly VerifiedIdService _verifiedIdService;
        private readonly IHubContext<VerifiedIdHub, IVerifiedIdHub> _hubContext;
        private readonly VerifiedIdSignalRRepository _verifiedIdSignalRRepository;

        public VerifiedIdController(VerifiedIdService verifiedIdService, IHubContext<VerifiedIdHub, IVerifiedIdHub> hubContext, VerifiedIdSignalRRepository verifiedIdSignalRRepository)
        {
            _verifiedIdService = verifiedIdService;
            _hubContext = hubContext;
            _verifiedIdSignalRRepository = verifiedIdSignalRRepository;
        }

        [Authorize(AuthenticationSchemes = Strings.VERIFIED_ID_CALLBACK_SCHEMA)]
        [HttpPost("callback")]
        public async Task<ActionResult> PresentationRequestCallback()
        {
            var userIdClaim = User.Claims.Where(claim => claim.Type == "userId").FirstOrDefault();

            var userId = userIdClaim?.Value;
            if (userId != null && _verifiedIdSignalRRepository.TryGetConnections(userId, out var connections))
            {
                await _hubContext.Clients.Clients(connections).HideQrCode();
            }

            using StreamReader streamReader = new StreamReader(Request.Body);
            var callbackBody = await streamReader.ReadToEndAsync();

            var parsedBody = JsonSerializer.Deserialize<CreatePresentationRequestCallback>(callbackBody);

            if (parsedBody?.RequestStatus == "request_retrieved")
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status204NoContent);
        }

        //[Authorize(Roles = "MyAccount.VNext.VerifiedId.Verify")]
        [HttpPost("verify")]
        public async Task<ActionResult> VerifyUser()
        {
            var userId = User.GetObjectId();

            if (userId == null)
            {
                return Unauthorized();
            }


            var response = await _verifiedIdService.CreatePresentationRequest(userId);
            return Ok(response);
        }
    }
}
