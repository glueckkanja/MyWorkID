using c4a8.MyWorkID.Server.Common;
using c4a8.MyWorkID.Server.Features.VerifiedId.Exceptions;
using c4a8.MyWorkID.Server.Features.VerifiedId.Extensions;
using c4a8.MyWorkID.Server.Filters;
using System.Security.Claims;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.Commands
{
    public class RequestPresentation : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPostWithUpdatedOpenApi("api/me/verifiedid/callback", HandleAsync)
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG)
                .RequireAuthorization(Strings.VERIFIED_ID_CALLBACK_SCHEMA)
                .AddEndpointFilter<CheckForUserIdEndpointFilter>();
        }

        public static async Task<IResult> HandleAsync(
            VerifiedIdService verifiedIdService,
            ClaimsPrincipal user,
            HttpContext context,
            CancellationToken cancellationToken)
        {
            var userId = user.GetUserId();
            await verifiedIdService.HideQrCodeForUser(userId!);
            try
            {
                var parsedBody = await verifiedIdService.ParseCreatePresentationRequestCallback(context);
                await verifiedIdService.HandlePresentationCallback(userId!, parsedBody);
                return TypedResults.NoContent();
            }
            catch (CreatePresentationException)
            {
                return TypedResults.BadRequest();
            }
        }
    }
}
