using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using MyWorkID.Server.Common;
using MyWorkID.Server.Features.VerifiedId.Entities;
using MyWorkID.Server.Features.VerifiedId.Exceptions;
using MyWorkID.Server.Filters;

namespace MyWorkID.Server.Features.VerifiedId.Commands
{
    /// <summary>
    /// Handles the creation of a QR code for verified ids.
    /// </summary>
    public class ValidateIdentity : IEndpoint
    {
        /// <summary>
        /// Maps the endpoint for the creation of a QR code for verified ids.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPostWithCreatedOpenApi("api/me/verifiedId/verify", HandleAsync)
                .RequireAuthorization()
                .AddEndpointFilter<CheckVerifiedIdAppConfigurationEndpointFilter>()
                .AddEndpointFilter<CheckForObjectIdEndpointFilter>()
                .WithTags(Strings.VERIFIEDID_OPENAPI_TAG);
        }

        /// <summary>
        /// Handles the request to create a QR code for verified ids.
        /// </summary>
        /// <param name="verifiedIdService">The verified ID service.</param>
        /// <param name="user">The claims principal representing the user.</param>
        /// <param name="action">Optional. The action the verification is started for. When every action requires its own verification, the verification only unlocks this action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result indicating the success or failure of the identity validation request.</returns>
        [Authorize(Roles = Strings.VALIDATE_IDENTITY_ROLE)]
        public static async Task<IResult> HandleAsync(
            VerifiedIdService verifiedIdService,
            ClaimsPrincipal user,
            [FromQuery] string? action,
            CancellationToken cancellationToken
        )
        {
            string? userId = user.GetObjectId();
            VerifiedIdAction? verifiedIdAction = null;
            if (action is not null)
            {
                if (!VerificationAttributeValue.TryParseAction(action, out VerifiedIdAction parsedAction))
                {
                    return TypedResults.Problem(
                        Strings.ERROR_VERIFIED_ID_UNKNOWN_ACTION,
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }
                verifiedIdAction = parsedAction;
            }
            try
            {
                CreatePresentationResponse? response =
                    await verifiedIdService.CreatePresentationRequest(
                        userId!,
                        verifiedIdAction,
                        cancellationToken
                    );
                return TypedResults.Created(string.Empty, response);
            }
            catch (CreatePresentationException)
            {
                return TypedResults.Problem();
            }
            catch (PremiumFeatureBillingMissingException)
            {
                return TypedResults.Problem(
                    Strings.PREMIUM_FEATURES_BILLING_MISSING_PROBLEM_DETAIL
                );
            }
        }
    }
}
