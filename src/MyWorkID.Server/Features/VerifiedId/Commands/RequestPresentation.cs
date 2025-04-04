﻿using MyWorkID.Server.Common;
using MyWorkID.Server.Features.VerifiedId.Exceptions;
using MyWorkID.Server.Features.VerifiedId.Extensions;
using MyWorkID.Server.Filters;
using System.Security.Claims;

namespace MyWorkID.Server.Features.VerifiedId.Commands
{
    /// <summary>
    /// Handles the request for presenting a verified ID.
    /// </summary>
    public class RequestPresentation : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for requesting a verified ID presentation.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPostWithUpdatedOpenApi("api/me/verifiedid/callback", HandleAsync)
                .RequireAuthorization(Strings.VERIFIED_ID_CALLBACK_POLICY)
                .AddEndpointFilter<CheckVerifiedIdAppConfigurationEndpointFilter>()
                .AddEndpointFilter<CheckForUserIdEndpointFilter>()
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to present a verified ID.
        /// </summary>
        /// <param name="verifiedIdService">The verified ID service.</param>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="context">The HTTP context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success or failure of the presentation request.</returns>
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