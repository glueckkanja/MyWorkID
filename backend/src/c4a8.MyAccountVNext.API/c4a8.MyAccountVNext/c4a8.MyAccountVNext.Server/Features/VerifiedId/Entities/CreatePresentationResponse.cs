using System.Text.Json.Serialization;

namespace c4a8.MyAccountVNext.Server.Features.VerifiedId.Entities
{
    public class CreatePresentationResponse
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("expiry")]
        public long? ExpiryDate { get; set; }
        [JsonPropertyName("qrCode")]
        public string? QrCodeBase64 { get; set; }

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
