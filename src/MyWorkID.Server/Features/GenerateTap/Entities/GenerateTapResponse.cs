using System.Text.Json.Serialization;

namespace c4a8.MyWorkID.Server.Features.GenerateTap.Entities
{
    /// <summary>
    /// Represents the response containing the generated Temporary Access Pass (TAP).
    /// </summary>
    public class GenerateTapResponse
    {
        /// <summary>
        /// Gets or sets the generated Temporary Access Pass.
        /// </summary>
        [JsonPropertyName("temporaryAccessPassword")]
        public string TemporaryAccessPassword { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateTapResponse"/> class.
        /// </summary>
        /// <param name="temporaryAccessPassword">The generated Temporary Access Pass.</param>
        public GenerateTapResponse(string temporaryAccessPassword)
        {
            TemporaryAccessPassword = temporaryAccessPassword;
        }
    }
}
