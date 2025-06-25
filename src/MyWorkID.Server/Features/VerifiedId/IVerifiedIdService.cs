using MyWorkID.Server.Features.VerifiedId.Entities;
using Microsoft.Graph.Models;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Interface for VerifiedID service operations.
    /// </summary>
    public interface IVerifiedIdService
    {
        /// <summary>
        /// Creates a presentation request for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response of the create presentation request.</returns>
        Task<CreatePresentationResponse?> CreatePresentationRequest(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Hides the QR code for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        Task HideQrCodeForUser(string userId);

        /// <summary>
        /// Parses the create presentation request callback from the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The parsed create presentation request callback.</returns>
        Task<CreatePresentationRequestCallback> ParseCreatePresentationRequestCallback(HttpContext context);

        /// <summary>
        /// Handles the presentation callback for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="callbackBody">The callback body.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HandlePresentationCallback(string userId, CreatePresentationRequestCallback callbackBody);

        /// <summary>
        /// Creates the request body for setting the target security attribute.
        /// </summary>
        /// <param name="targetSecurityAttributeValue">The value of the target security attribute.</param>
        /// <returns>The user object with the target security attribute set.</returns>
        User CreateSetTargetSecurityAttributeRequestBody(string targetSecurityAttributeValue);

        /// <summary>
        /// Checks if the user has a recent VerifiedID validation within the configured time window.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the user has a recent VerifiedID validation, false otherwise.</returns>
        Task<bool> HasRecentVerifiedId(string userId, CancellationToken cancellationToken);
    }
}