using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Options
{
    /// <summary>
    /// Configuration options for Verified ID operations.
    /// </summary>
    public class VerifiedIdOptions
    {
        /// <summary>
        /// Gets or sets the decentralized identifier.
        /// </summary>
        [Required]
        public string? DecentralizedIdentifier { get; set; } // https://portal.azure.com/#view/Microsoft_AAD_DecentralizedIdentity/InitialMenuBlade/~/issuerSettingsBlade

        /// <summary>
        /// Gets or sets the backend URL.
        /// </summary>
        [Required]
        public string? BackendUrl { get; set; }

        /// <summary>
        /// Gets or sets the JWT signing key.
        /// </summary>
        [Required]
        public string? JwtSigningKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable hiding the QR code.
        /// </summary>
        public bool DisableQrCodeHide { get; set; } = false;

        /// <summary>
        /// Gets or sets the target security attribute set.
        /// </summary>
        [Required]
        public string? TargetSecurityAttributeSet { get; set; }

        /// <summary>
        /// Gets or sets the target security attribute.
        /// </summary>
        [Required]
        public string? TargetSecurityAttribute { get; set; }

        /// <summary>
        /// Gets or sets the URI for creating a presentation request.
        /// </summary>
        [Required]
        public string? CreatePresentationRequestUri { get; set; }

        /// <summary>
        /// Gets or sets the time window in minutes for which a VerifiedID is considered recent.
        /// Default is 30 minutes.
        /// </summary>
        public int RequiredVerificationTimeWindowMinutes { get; set; } = 30;
    }
}
