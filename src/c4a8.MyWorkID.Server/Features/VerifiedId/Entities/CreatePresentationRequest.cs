using System.Text.Json.Serialization;

namespace c4a8.MyWorkID.Server.Features.VerifiedId.Entities
{
    public class RequestRegistration
    {
        [JsonPropertyName("clientName")]
        public string ClientName { get; set; }
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }
        [JsonPropertyName("logoUrl")]
        public string? LogoUrl { get; set; }
        [JsonPropertyName("termsOfServiceUrl")]
        public string? TermsOfServiceUrl { get; set; }

        [JsonConstructor]
        public RequestRegistration(string clientName, string? purpose = null, string? logoUrl = null, string? termsOfServiceUrl = null)
        {
            ClientName = clientName;
            Purpose = purpose;
            LogoUrl = logoUrl;
            TermsOfServiceUrl = termsOfServiceUrl;
        }
    }
    public class Callback
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; }

        [JsonConstructor]
        public Callback(string url, string state, Dictionary<string, string>? headers = null)
        {
            Url = url;
            State = state;
            Headers = headers;
        }
    }
    public class FaceCheck
    {
        [JsonPropertyName("sourcePhotoClaimName")]
        public string SourcePhotoClaimName { get; set; }
        [JsonPropertyName("matchConfidenceThreshold")]
        public int? MatchConfidenceThreshold { get; set; }

        [JsonConstructor]
        public FaceCheck(string sourcePhotoClaimName, int matchConfidenceThreshold = 70)
        {
            SourcePhotoClaimName = sourcePhotoClaimName;
            MatchConfidenceThreshold = matchConfidenceThreshold;
        }
    }

    public class Validation
    {
        [JsonPropertyName("allowRevoked")]
        public bool? AllowRevoked { get; set; }
        [JsonPropertyName("validateLinkedDomain")]
        public bool? ValidateLinkedDomain { get; set; }
        [JsonPropertyName("faceCheck")]
        public FaceCheck? FaceCheck { get; set; }

        [JsonConstructor]
        public Validation(bool? allowRevoked = false, bool? validateLinkedDomain = false, FaceCheck? faceCheck = null)
        {
            AllowRevoked = allowRevoked;
            ValidateLinkedDomain = validateLinkedDomain;
            FaceCheck = faceCheck;
        }
    }

    public class Configuration
    {
        [JsonPropertyName("validation")]
        public Validation Validation { get; set; }

        [JsonConstructor]
        public Configuration(Validation validation)
        {
            Validation = validation;
        }
    }

    public class Constraints
    {
        [JsonPropertyName("claimName")]
        public string ClaimName { get; set; }
        [JsonPropertyName("values")]
        public IEnumerable<string>? Values { get; set; }
        [JsonPropertyName("contains")]
        public string? Contains { get; set; }
        [JsonPropertyName("startsWith")]
        public string? StartsWith { get; set; }

        [JsonConstructor]
        public Constraints(string claimName, IEnumerable<string>? values = null, string? contains = null, string? startsWith = null)
        {
            ClaimName = claimName;
            Values = values;
            Contains = contains;
            StartsWith = startsWith;
        }

    }
    public class RequestCredential
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }
        [JsonPropertyName("acceptedIssuers")]
        public IEnumerable<string>? AcceptedIssuers { get; set; }
        [JsonPropertyName("configuration")]
        public Configuration? Configuration { get; set; }
        [JsonPropertyName("constraints")]
        public Constraints? Constraints { get; set; }

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

    public class CreatePresentationRequest
    {
        [JsonPropertyName("includeQRCode")]
        public bool? IncludeQRCode { get; set; }
        [JsonPropertyName("includeReceipt")]
        public bool? IncludeReceipt { get; set; }
        [JsonPropertyName("authority")]
        public string Authority { get; set; }
        [JsonPropertyName("registration")]
        public RequestRegistration Registration { get; set; }
        [JsonPropertyName("callback")]
        public Callback Callback { get; set; }
        [JsonPropertyName("requestedCredentials")]
        public IEnumerable<RequestCredential> RequestedCredentials { get; set; }

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
