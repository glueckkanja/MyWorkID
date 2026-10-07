namespace MyWorkID.Server.IntegrationTests.Features.Configuration
{
    public class GetFrontendConfigTestResponse
    {
        public string? FrontendClientId { get; set; }
        public string? TenantId { get; set; }
        public string? BackendClientId { get; set; }
        public string? CustomCssUrl { get; set; }
        public string? AppTitle { get; set; }
        public string? FaviconUrl { get; set; }
        public string? HelpUrl { get; set; }
        public string? MaxDismissibleRiskLevel { get; set; }
    }
}
