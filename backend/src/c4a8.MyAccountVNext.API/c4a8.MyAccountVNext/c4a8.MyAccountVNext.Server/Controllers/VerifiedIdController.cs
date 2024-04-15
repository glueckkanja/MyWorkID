using c4a8.MyAccountVNext.Server.Hubs;
using c4a8.MyAccountVNext.Server.Models.VerifiedId;
using c4a8.MyAccountVNext.Server.Options;
using c4a8.MyAccountVNext.Server.Repositories;
using c4a8.MyAccountVNext.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
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
        private readonly VerifiedIdOptions _verifiedIdOptions;

        public VerifiedIdController(VerifiedIdService verifiedIdService, IHubContext<VerifiedIdHub, IVerifiedIdHub> hubContext, VerifiedIdSignalRRepository verifiedIdSignalRRepository, IOptions<VerifiedIdOptions> verifiedIdOptions)
        {
            _verifiedIdService = verifiedIdService;
            _hubContext = hubContext;
            _verifiedIdSignalRRepository = verifiedIdSignalRRepository;
            _verifiedIdOptions = verifiedIdOptions.Value;
        }

        [Authorize(AuthenticationSchemes = Strings.VERIFIED_ID_CALLBACK_SCHEMA)]
        [HttpPost("callback")]
        public async Task<ActionResult> PresentationRequestCallback()
        {
            var userIdClaim = User.Claims.Where(claim => claim.Type == "userId").FirstOrDefault();

            string? userId = userIdClaim?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (!_verifiedIdOptions.DisableQrCodeHide && _verifiedIdSignalRRepository.TryGetConnections(userId, out var connections))
            {
                await _hubContext.Clients.Clients(connections).HideQrCode();
            }

            using StreamReader streamReader = new StreamReader(Request.Body);
            var callbackBody = await streamReader.ReadToEndAsync();

            CreatePresentationRequestCallback? parsedBody = null;

            try
            {
                parsedBody = JsonSerializer.Deserialize<CreatePresentationRequestCallback>(callbackBody);
            }
            catch (Exception e)
            {
                if (e is JsonException || e is ArgumentNullException)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "Invalid body");
                }
                else
                {
                    throw;
                }
            }

            if (parsedBody == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid body");
            }

            await _verifiedIdService.HandlePresentationCallback(userId, parsedBody);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [Authorize(Roles = "MyAccount.VNext.ValidateIdentity")]
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
