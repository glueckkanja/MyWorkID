using System.Text.Json.Serialization;

namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// Represents the registration details for a presentation request.
    /// </summary>
    public class RequestRegistration
    {
        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        [JsonPropertyName("clientName")]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the purpose of the request.
        /// </summary>
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets the URL of the client's logo.
        /// </summary>
        [JsonPropertyName("logoUrl")]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the terms of service.
        /// </summary>
        [JsonPropertyName("termsOfServiceUrl")]
        public string? TermsOfServiceUrl { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestRegistration"/> class.
        /// </summary>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="purpose">The purpose of the request.</param>
        /// <param name="logoUrl">The URL of the client's logo.</param>
        /// <param name="termsOfServiceUrl">The URL of the terms of service.</param>
        [JsonConstructor]
        public RequestRegistration(string clientName, string? purpose = null, string? logoUrl = null, string? termsOfServiceUrl = null)
        {
            ClientName = clientName;
            Purpose = purpose;
            LogoUrl = logoUrl;
            TermsOfServiceUrl = termsOfServiceUrl;
        }
    }
    /// <summary>
    /// Represents the callback details for a presentation request.
    /// </summary>
    public class Callback
    {
        /// <summary>
        /// Gets or sets the URL to which the callback should be sent.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the state information to be included in the callback.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the headers to be included in the callback request.
        /// </summary>
        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Callback"/> class.
        /// </summary>
        /// <param name="url">The URL to which the callback should be sent.</param>
        /// <param name="state">The state information to be included in the callback.</param>
        /// <param name="headers">The headers to be included in the callback request.</param>
        [JsonConstructor]
        public Callback(string url, string state, Dictionary<string, string>? headers = null)
        {
            Url = url;
            State = state;
            Headers = headers;
        }
    }
    /// <summary>
    /// Represents the face check details for a presentation request.
    /// </summary>
    public class FaceCheck
    {
        /// <summary>
        /// Gets or sets the name of the claim that contains the source photo.
        /// </summary>
        [JsonPropertyName("sourcePhotoClaimName")]
        public string SourcePhotoClaimName { get; set; }

        /// <summary>
        /// Gets or sets the match confidence threshold for the face check.
        /// </summary>
        [JsonPropertyName("matchConfidenceThreshold")]
        public int? MatchConfidenceThreshold { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FaceCheck"/> class.
        /// </summary>
        /// <param name="sourcePhotoClaimName">The name of the claim that contains the source photo.</param>
        /// <param name="matchConfidenceThreshold">The match confidence threshold for the face check.</param>
        [JsonConstructor]
        public FaceCheck(string sourcePhotoClaimName, int matchConfidenceThreshold = 70)
        {
            SourcePhotoClaimName = sourcePhotoClaimName;
            MatchConfidenceThreshold = matchConfidenceThreshold;
        }
    }

    /// <summary>
    /// Represents the validation details for a presentation request.
    /// </summary>
    public class Validation
    {
        /// <summary>
        /// Gets or sets a value indicating whether revoked credentials are allowed.
        /// </summary>
        [JsonPropertyName("allowRevoked")]
        public bool? AllowRevoked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate the linked domain.
        /// </summary>
        [JsonPropertyName("validateLinkedDomain")]
        public bool? ValidateLinkedDomain { get; set; }

        /// <summary>
        /// Gets or sets the face check details.
        /// </summary>
        [JsonPropertyName("faceCheck")]
        public FaceCheck? FaceCheck { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Validation"/> class.
        /// </summary>
        /// <param name="allowRevoked">Indicates whether revoked credentials are allowed.</param>
        /// <param name="validateLinkedDomain">Indicates whether to validate the linked domain.</param>
        /// <param name="faceCheck">The face check details.</param>
        [JsonConstructor]
        public Validation(bool? allowRevoked = false, bool? validateLinkedDomain = false, FaceCheck? faceCheck = null)
        {
            AllowRevoked = allowRevoked;
            ValidateLinkedDomain = validateLinkedDomain;
            FaceCheck = faceCheck;
        }
    }

    /// <summary>
    /// Represents the configuration details for a presentation request.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the validation details.
        /// </summary>
        [JsonPropertyName("validation")]
        public Validation Validation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        /// <param name="validation">The validation details.</param>
        [JsonConstructor]
        public Configuration(Validation validation)
        {
            Validation = validation;
        }
    }

    /// <summary>
    /// Represents the constraints for a presentation request.
    /// </summary>
    public class Constraints
    {
        /// <summary>
        /// Gets or sets the name of the claim to be constrained.
        /// </summary>
        [JsonPropertyName("claimName")]
        public string ClaimName { get; set; }

        /// <summary>
        /// Gets or sets the values that the claim must match.
        /// </summary>
        [JsonPropertyName("values")]
        public IEnumerable<string>? Values { get; set; }

        /// <summary>
        /// Gets or sets the substring that the claim must contain.
        /// </summary>
        [JsonPropertyName("contains")]
        public string? Contains { get; set; }

        /// <summary>
        /// Gets or sets the prefix that the claim must start with.
        /// </summary>
        [JsonPropertyName("startsWith")]
        public string? StartsWith { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Constraints"/> class.
        /// </summary>
        /// <param name="claimName">The name of the claim to be constrained.</param>
        /// <param name="values">The values that the claim must match.</param>
        /// <param name="contains">The substring that the claim must contain.</param>
        /// <param name="startsWith">The prefix that the claim must start with.</param>
        [JsonConstructor]
        public Constraints(string claimName, IEnumerable<string>? values = null, string? contains = null, string? startsWith = null)
        {
            ClaimName = claimName;
            Values = values;
            Contains = contains;
            StartsWith = startsWith;
        }
    }
    /// <summary>
    /// Represents the details of a credential requested in a presentation request.
    /// </summary>
    public class RequestCredential
    {
        /// <summary>
        /// Gets or sets the type of the requested credential.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the purpose of the requested credential.
        /// </summary>
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets the list of accepted issuers for the requested credential.
        /// </summary>
        [JsonPropertyName("acceptedIssuers")]
        public IEnumerable<string>? AcceptedIssuers { get; set; }

        /// <summary>
        /// Gets or sets the configuration details for the requested credential.
        /// </summary>
        [JsonPropertyName("configuration")]
        public Configuration? Configuration { get; set; }

        /// <summary>
        /// Gets or sets the constraints for the requested credential.
        /// </summary>
        [JsonPropertyName("constraints")]
        public Constraints? Constraints { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestCredential"/> class.
        /// </summary>
        /// <param name="type">The type of the requested credential.</param>
        /// <param name="purpose">The purpose of the requested credential.</param>
        /// <param name="acceptedIssuers">The list of accepted issuers for the requested credential.</param>
        /// <param name="configuration">The configuration details for the requested credential.</param>
        /// <param name="constraints">The constraints for the requested credential.</param>
        [JsonConstructor]
        public RequestCredential(string type, string? purpose = null, IEnumerable<string>? acceptedIssuers = null, Configuration? configuration = null, Constraints? constraints = null)
        {
            Type = type;
            Purpose = purpose;
            AcceptedIssuers = acceptedIssuers;
            Configuration = configuration;
            Constraints = constraints;
        }
    }

    // Other classes...

    /// <summary>
    /// Represents the details required to create a presentation request.
    /// </summary>
    public class CreatePresentationRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include a QR code in the presentation request.
        /// </summary>
        [JsonPropertyName("includeQRCode")]
        public bool? IncludeQRCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include a receipt in the presentation request.
        /// </summary>
        [JsonPropertyName("includeReceipt")]
        public bool? IncludeReceipt { get; set; }

        /// <summary>
        /// Gets or sets the authority issuing the presentation request.
        /// </summary>
        [JsonPropertyName("authority")]
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the registration details for the presentation request.
        /// </summary>
        [JsonPropertyName("registration")]
        public RequestRegistration Registration { get; set; }

        /// <summary>
        /// Gets or sets the callback details for the presentation request.
        /// </summary>
        [JsonPropertyName("callback")]
        public Callback Callback { get; set; }

        /// <summary>
        /// Gets or sets the list of credentials requested in the presentation request.
        /// </summary>
        [JsonPropertyName("requestedCredentials")]
        public IEnumerable<RequestCredential> RequestedCredentials { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePresentationRequest"/> class.
        /// </summary>
        /// <param name="authority">The authority issuing the presentation request.</param>
        /// <param name="registration">The registration details for the presentation request.</param>
        /// <param name="callback">The callback details for the presentation request.</param>
        /// <param name="requestedCredentials">The list of credentials requested in the presentation request.</param>
        /// <param name="includeQRCode">Indicates whether to include a QR code in the presentation request.</param>
        /// <param name="includeReceipt">Indicates whether to include a receipt in the presentation request.</param>
        [JsonConstructor]
        public CreatePresentationRequest(string authority, RequestRegistration registration, Callback callback, IEnumerable<RequestCredential> requestedCredentials, bool? includeQRCode = true, bool? includeReceipt = false)
        {
            IncludeQRCode = includeQRCode;
            IncludeReceipt = includeReceipt;
            Authority = authority;
            Registration = registration;
            Callback = callback;
            RequestedCredentials = requestedCredentials;
        }
    }
}
