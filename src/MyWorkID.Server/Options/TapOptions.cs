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
        /// Overrides the TAP lifetime in minutes. Graph enforces 10-43200 minutes.
        /// </summary>
        [Range(10, 43200, ErrorMessage = "The field 'LifetimeInMinutes' must be between 10 and 43200.")]
        public int? LifetimeInMinutes { get; set; }

        /// <summary>
        /// Determines whether the generated TAP can be used only once.
        /// </summary>
        public bool? IsUsableOnce { get; set; }
    }
}
