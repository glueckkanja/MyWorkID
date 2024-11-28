using c4a8.MyAccountVNext.Server.Common;
using c4a8.MyAccountVNext.Server.Features.VerifiedId.Entities;
using c4a8.MyAccountVNext.Server.Features.VerifiedId.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Security.Claims;
using System.Text.Json;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId.Commands
{
    public class RequestPresentation : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPostWithUpdatedOpenApi("api/me/verifiedid/callback", HandleAsync)
                .RequireAuthorization(Strings.VERIFIED_ID_CALLBACK_SCHEMA)
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
        }

        public static async Task<IResult> HandleAsync(
            IVerifiedIdSignalRRepository verifiedIdSignalRRepository,
            VerifiedIdService verifiedIdService,
            IHubContext<VerifiedIdHub, IVerifiedIdHub> hubContext,
            IOptions<VerifiedIdOptions> verifiedIdIOptions,
            ClaimsPrincipal user,
            HttpContext context,
            GraphServiceClient graphClient,
            IAuthContextService authContextService,
            CancellationToken cancellationToken)
        {
            var userIdClaim = user.Claims.Where(claim => claim.Type == "userId").FirstOrDefault();
            string? userId = userIdClaim?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return TypedResults.StatusCode(StatusCodes.Status401Unauthorized);
            }
            var verifiedIdOptions = verifiedIdIOptions.Value;
            if (!verifiedIdOptions.DisableQrCodeHide && verifiedIdSignalRRepository.TryGetConnections(userId, out var connections))
            {
                await hubContext.Clients.Clients(connections).HideQrCode();
            }

            using StreamReader streamReader = new StreamReader(context.Request.Body);
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
                    return TypedResults.BadRequest(Strings.ERROR_INVALID_BODY);
                }
                else
                {
                    throw;
                }
            }

            if (parsedBody == null)
            {
                return TypedResults.BadRequest(Strings.ERROR_INVALID_BODY);
            }

            await verifiedIdService.HandlePresentationCallback(userId, parsedBody);

            return TypedResults.NoContent();
        }
    }
}
