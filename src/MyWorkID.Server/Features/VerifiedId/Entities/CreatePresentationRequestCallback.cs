using System.Text.Json.Serialization;

namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    // Other classes...

    /// <summary>
    /// Represents an error that occurred during the creation of a presentation request.
    /// </summary>
    public class CreatePresentationRequestError
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    // Other classes...

    /// <summary>
    /// Represents the data of verified credentials in a presentation request callback.
    /// </summary>
    public class VerifiedCredentialsData
    {
        /// <summary>
        /// Gets or sets the issuer of the verified credential.
        /// </summary>
        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        /// <summary>
        /// Gets or sets the types of the verified credential.
        /// </summary>
        [JsonPropertyName("type")]
        public IEnumerable<string>? Type { get; set; }

        /// <summary>
        /// Gets or sets the claims of the verified credential.
        /// </summary>
        [JsonPropertyName("claims")]
        public Dictionary<string, string>? Claims { get; set; }

        /// <summary>
        /// Gets or sets the state of the verified credential.
        /// </summary>
        [JsonPropertyName("credentialState")]
        public Dictionary<string, string>? CredentialState { get; set; }

        /// <summary>
        /// Gets or sets the domain validation details of the verified credential.
        /// </summary>
        [JsonPropertyName("domainValidation")]
        public Dictionary<string, string>? DomainValidation { get; set; }

        /// <summary>
        /// Gets or sets the issuance date of the verified credential.
        /// </summary>
        [JsonPropertyName("issuanceDate")]
        public DateTime? IssuanceDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the verified credential.
        /// </summary>
        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; set; }
    }

    /// <summary>
    /// Represents the callback details for a presentation request.
    /// </summary>
    public class CreatePresentationRequestCallback
    {
        /// <summary>
        /// Gets or sets the ID of the presentation request.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets or sets the status of the presentation request.
        /// </summary>
        [JsonPropertyName("requestStatus")]
        public string? RequestStatus { get; set; }

        /// <summary>
        /// Gets or sets the state information associated with the presentation request.
        /// </summary>
        [JsonPropertyName("state")]
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the subject of the presentation request.
        /// </summary>
        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the data of verified credentials in the presentation request.
        /// </summary>
        [JsonPropertyName("verifiedCredentialsData")]
        public IEnumerable<VerifiedCredentialsData>? VerifiedCredentialsData { get; set; }

        /// <summary>
        /// Gets or sets the error details if an error occurred during the presentation request.
        /// </summary>
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CreatePresentationRequestError? Error { get; set; }
    }
}
