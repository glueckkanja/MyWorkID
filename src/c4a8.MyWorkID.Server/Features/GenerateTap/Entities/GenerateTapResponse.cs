using System.Text.Json.Serialization;

namespace c4a8.MyWorkID.Server.Features.GenerateTap.Entities
{
    public class GenerateTapResponse
    {
        [JsonPropertyName("temporaryAccessPassword")]
        public string TemporaryAccessPassword { get; set; }

        public GenerateTapResponse(string temporaryAccessPassword)
        {
            TemporaryAccessPassword = temporaryAccessPassword;
        }
    }
}
