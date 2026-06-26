using System.Text.Json.Serialization;

namespace MyWorkID.Server.Features.GenerateTap.Entities
{
    /// <summary>
    /// Represents the response containing the generated Temporary Access Pass (TAP)
    /// as well as the unique identifier of the TAP.
    /// </summary>
    public class GenerateTapResponse
    {
        [JsonPropertyName("temporaryAccessPassId")]
        public string TemporaryAccessPassId { get; set; }

        [JsonPropertyName("temporaryAccessPassword")]
        public string TemporaryAccessPassword { get; set; }

        public GenerateTapResponse(string temporaryAccessPassId, string temporaryAccessPassword)
        {
            TemporaryAccessPassId = temporaryAccessPassId;
            TemporaryAccessPassword = temporaryAccessPassword;
        }
    }
}
