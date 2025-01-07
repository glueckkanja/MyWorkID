using c4a8.MyWorkID.Server.Validation;

namespace c4a8.MyWorkID.Server.Options
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
