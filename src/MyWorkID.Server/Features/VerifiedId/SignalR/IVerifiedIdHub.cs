namespace MyWorkID.Server.Features.VerifiedId.SignalR
{
    /// <summary>
    /// Defines the SignalR hub interface for Verified ID operations.
    /// Every event is sent to all connections of the user, so it carries the ID of the presentation
    /// request it belongs to. Clients only act on the presentation they started themselves.
    /// </summary>
    public interface IVerifiedIdHub
    {
        /// <summary>
        /// Hides the QR code on the client side.
        /// </summary>
        /// <param name="requestId">The ID of the presentation request whose QR code was scanned.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HideQrCode(string? requestId);

        /// <summary>
        /// Notifies the client that the verification was successful.
        /// </summary>
        /// <param name="requestId">The ID of the presentation request that was verified.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task VerificationSuccess(string? requestId);

        /// <summary>
        /// Notifies the client that the verification failed.
        /// </summary>
        /// <param name="requestId">The ID of the presentation request that failed.</param>
        /// <param name="errorMessage">The error message describing why verification failed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task VerificationFailed(string? requestId, string errorMessage);
    }
}
