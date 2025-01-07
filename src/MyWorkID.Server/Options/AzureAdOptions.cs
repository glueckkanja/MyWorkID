using MyWorkID.Server.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Options
{
    public class AzureAdOptions : BaseOptions
    {
        public const string SectionName = "AzureAd";

        [Required]
        public string? Instance { get; set; }
        [Guid]
        public string? TenantId { get; set; }
        [Guid]
        public string? ClientId { get; set; }
    }
}
