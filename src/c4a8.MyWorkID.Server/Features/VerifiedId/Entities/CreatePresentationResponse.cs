using System.Text.Json.Serialization;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// Represents the response received after creating a presentation request.
    /// </summary>
    public class CreatePresentationResponse
    {
        /// <summary>
        /// Gets or sets the ID of the presentation request.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the URL for the presentation request.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the expiry date of the presentation request in Unix time.
        /// </summary>
        [JsonPropertyName("expiry")]
        public long? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the QR code for the presentation request in Base64 format.
        /// </summary>
        [JsonPropertyName("qrCode")]
        public string? QrCodeBase64 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePresentationResponse"/> class.
        /// </summary>
        /// <param name="requestId">The ID of the presentation request.</param>
        /// <param name="url">The URL for the presentation request.</param>
        /// <param name="expiryDate">The expiry date of the presentation request in Unix time.</param>
        /// <param name="qrCodeBase64">The QR code for the presentation request in Base64 format.</param>
        [JsonConstructor]
        public CreatePresentationResponse(string? requestId, string? url, long? expiryDate, string? qrCodeBase64)
        {
            RequestId = requestId;
            Url = url;
            ExpiryDate = expiryDate;
            QrCodeBase64 = qrCodeBase64;
        }
    }
}
