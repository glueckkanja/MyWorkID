namespace c4a8.MyAccountVNext.Server.Options
{
    public class VerifiedIdOptions
    {
        public string? DecentralizedIdentifier { get; set; } // https://portal.azure.com/#view/Microsoft_AAD_DecentralizedIdentity/InitialMenuBlade/~/issuerSettingsBlade
        public string? BackendUrl { get; set; }
        public string? JwtSigningKey { get; set; }
        public bool DisableQrCodeHide { get; set; } = false;
        public string? TargetSecurityPropertySet { get; set; }
        public string? TargetSecurityProperty { get; set; }
    }
}
