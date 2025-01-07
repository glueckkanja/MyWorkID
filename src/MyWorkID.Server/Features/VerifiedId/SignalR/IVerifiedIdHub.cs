namespace c4a8.MyWorkID.Server.Features.VerifiedId.SignalR
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
    }
}