using MyWorkID.Server.Validation;

namespace MyWorkID.Server.Options
{
    /// <summary>
    /// Represents the configuration options for the frontend application.
    /// </summary>
    public class FrontendOptions : BaseOptions
    {
        public const string SectionName = "Frontend";

        /// <summary>
        /// Client ID for the frontend application.
        /// </summary>
        [Guid]
        public string? FrontendClientId { get; set; }

        /// <summary>
        /// Tenant ID.
        /// </summary>
        [Guid]
        public string? TenantId { get; set; }

        /// <summary>
        /// Client ID for the backend application.
        /// </summary>
        [Guid]
        public string? BackendClientId { get; set; }

        /// <summary>
        /// Optional URL to a custom CSS file for theming.
        /// </summary>
        public string? CustomCssUrl { get; set; }
    }
}
