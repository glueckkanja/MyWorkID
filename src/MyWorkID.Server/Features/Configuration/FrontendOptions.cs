namespace MyWorkID.Server.Features.Configuration
{
    /// <summary>
    /// Represents the configuration options for the frontend application.
    /// </summary>
    public class FrontendOptions
    {
        /// <summary>
        /// Client ID for the frontend application.
        /// </summary>
        public string? FrontendClientId { get; set; }

        /// <summary>
        /// Tenant ID.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Client ID for the backend application.
        /// </summary>
        public string? BackendClientId { get; set; }
    }
}
