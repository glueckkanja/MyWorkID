using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Options
{
    /// <summary>
    /// Configuration options that optionally require a recent Verified ID verification before
    /// a user may perform sensitive self-service actions (reset password, generate a
    /// Temporary Access Pass, dismiss user risk).
    /// </summary>
    public class VerifiedIdRequirementOptions : BaseOptions
    {
        public const string SectionName = "VerifiedIdRequirement";

        /// <summary>
        /// Default validity of a verification when nothing is configured: 24 hours.
        /// </summary>
        public const int DEFAULT_VALIDITY_IN_MINUTES = 1440;

        /// <summary>
        /// Enables the requirement. When <c>false</c> (default) sensitive actions behave exactly as
        /// before and do not depend on a Verified ID verification.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// How long (in minutes) a successful Verified ID verification counts as recent.
        /// The timestamp of the last verification is stored in the custom security attribute configured
        /// under <c>VerifiedId:TargetSecurityAttributeSet</c> / <c>VerifiedId:TargetSecurityAttribute</c>.
        /// Defaults to 1440 minutes (24 hours).
        /// </summary>
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "The field 'ValidityInMinutes' must be a positive number of minutes."
        )]
        public int ValidityInMinutes { get; set; } = DEFAULT_VALIDITY_IN_MINUTES;

        /// <summary>
        /// The configured validity as a <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan Validity => TimeSpan.FromMinutes(ValidityInMinutes);
    }
}
