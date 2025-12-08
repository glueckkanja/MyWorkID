using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Options
{
    /// <summary>
    /// Provides configuration options for Temporary Access Pass (TAP) requests.
    /// </summary>
    public class TapOptions : BaseOptions
    {
        public const string SectionName = "Tap";

        /// <summary>
        /// Overrides the TAP lifetime in minutes. Graph API enforces 60-480 minutes.
        /// Note: Official Microsoft documentation may show different ranges, but the actual API validation requires 60-480.
        /// </summary>
        [Range(60, 480, ErrorMessage = "The field 'LifetimeInMinutes' must be between 60 and 480.")]
        public int? LifetimeInMinutes { get; set; }

        /// <summary>
        /// Determines whether the generated TAP can be used only once.
        /// </summary>
        public bool? IsUsableOnce { get; set; }
    }
}
