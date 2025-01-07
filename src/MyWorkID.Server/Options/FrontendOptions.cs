using MyWorkID.Server.Validation;

namespace MyWorkID.Server.Options
{
    public class FrontendOptions : BaseOptions
    {
        public const string SectionName = "Frontend";

        [Guid]
        public string? FrontendClientId { get; set; }
        [Guid]
        public string? TenantId { get; set; }
        [Guid]
        public string? BackendClientId { get; set; }
    }
}
