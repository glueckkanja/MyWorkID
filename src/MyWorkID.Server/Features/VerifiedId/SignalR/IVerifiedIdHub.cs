namespace MyWorkID.Server.Features.VerifiedId.SignalR
{
    /// <summary>
    /// Defines the SignalR hub interface for Verified ID operations.
    /// </summary>
    public interface IVerifiedIdHub
    {
        /// <summary>
        /// Hides the QR code on the client side.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task HideQrCode();

        /// <summary>
        /// Notifies the client that the verification was successful.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task VerificationSuccess();

        /// <summary>
        /// Notifies the client that the verification failed.
        /// </summary>
        /// <param name="errorMessage">The error message describing why verification failed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task VerificationFailed(string errorMessage);
    }
}