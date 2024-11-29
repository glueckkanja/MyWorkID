namespace c4a8.MyWorkID.Server.Features.VerifiedId
{
    public class VerifiedIdOptions
    {
        public string? DecentralizedIdentifier { get; set; } // https://portal.azure.com/#view/Microsoft_AAD_DecentralizedIdentity/InitialMenuBlade/~/issuerSettingsBlade
        public string? BackendUrl { get; set; }
        public string? JwtSigningKey { get; set; }
        public bool DisableQrCodeHide { get; set; } = false;
        public string? TargetSecurityAttributeSet { get; set; }
        public string? TargetSecurityAttribute { get; set; }
    }
}
