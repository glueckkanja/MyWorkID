using System.Text.Json.Serialization;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.Entities
{
    public class CreatePresentationRequestError
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class VerifiedCredentialsData
    {
        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }
        [JsonPropertyName("type")]
        public IEnumerable<string>? Type { get; set; }
        [JsonPropertyName("claims")]
        public Dictionary<string, string>? Claims { get; set; }
        [JsonPropertyName("credentialState")]
        public Dictionary<string, string>? CredentialState { get; set; }
        [JsonPropertyName("domainValidation")]
        public Dictionary<string, string>? DomainValidation { get; set; }
        [JsonPropertyName("issuanceDate")]
        public DateTime? IssuanceDate { get; set; }
        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; set; }
    }

    public class CreatePresentationRequestCallback
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }
        [JsonPropertyName("requestStatus")]
        public string? RequestStatus { get; set; }
        [JsonPropertyName("state")]
        public string? State { get; set; }
        [JsonPropertyName("subject")]
        public string? Subject { get; set; }
        [JsonPropertyName("verifiedCredentialsData")]
        public IEnumerable<VerifiedCredentialsData>? VerifiedCredentialsData { get; set; }
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CreatePresentationRequestError? Error { get; set; }
    }
}
