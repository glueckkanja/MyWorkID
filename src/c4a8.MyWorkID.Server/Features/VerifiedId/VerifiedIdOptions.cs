namespace c4a8.MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Configuration options for Verified ID operations.
    /// </summary>
    public class VerifiedIdOptions
    {
        /// <summary>
        /// Gets or sets the decentralized identifier.
        /// </summary>
        public string? DecentralizedIdentifier { get; set; } // https://portal.azure.com/#view/Microsoft_AAD_DecentralizedIdentity/InitialMenuBlade/~/issuerSettingsBlade

        /// <summary>
        /// Gets or sets the backend URL.
        /// </summary>
        public string? BackendUrl { get; set; }

        /// <summary>
        /// Gets or sets the JWT signing key.
        /// </summary>
        public string? JwtSigningKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable hiding the QR code.
        /// </summary>
        public bool DisableQrCodeHide { get; set; } = false;

        /// <summary>
        /// Gets or sets the target security attribute set.
        /// </summary>
        public string? TargetSecurityAttributeSet { get; set; }

        /// <summary>
        /// Gets or sets the target security attribute.
        /// </summary>
        public string? TargetSecurityAttribute { get; set; }

        /// <summary>
        /// Gets or sets the URI for creating a presentation request.
        /// </summary>
        public string? CreatePresentationRequestUri { get; set; }
    }
}
