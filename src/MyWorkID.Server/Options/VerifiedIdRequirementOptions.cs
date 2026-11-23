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
        /// How long (in minutes) a verification can be used for a single action when
        /// <see cref="ValidityInMinutes"/> is not configured or <c>0</c>. Leaves enough time to finish the
        /// action after the face check, e.g. to type a new password or complete a Conditional Access step-up.
        /// </summary>
        public const int PER_ACTION_VERIFICATION_WINDOW_IN_MINUTES = 10;

        /// <summary>
        /// Enables the requirement. When <c>false</c> (default) sensitive actions behave exactly as
        /// before and do not depend on a Verified ID verification.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// How long (in minutes) a successful Verified ID verification counts as recent.
        /// The timestamp of the last verification is stored in the custom security attribute configured
        /// under <c>VerifiedId:TargetSecurityAttributeSet</c> / <c>VerifiedId:TargetSecurityAttribute</c>.
        /// When not configured or <c>0</c>, every action requires its own verification
        /// (see <see cref="RequiresVerificationPerAction"/>).
        /// </summary>
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "The field 'ValidityInMinutes' must be zero or a positive number of minutes."
        )]
        public int ValidityInMinutes { get; set; }

        /// <summary>
        /// <c>true</c> if no validity window is configured, so a verification unlocks only a single action and
        /// must be used within <see cref="PER_ACTION_VERIFICATION_WINDOW_IN_MINUTES"/>.
        /// </summary>
        public bool RequiresVerificationPerAction => ValidityInMinutes == 0;

        /// <summary>
        /// How long a verification counts as recent as a <see cref="TimeSpan"/>: the configured validity, or the
        /// per-action window when every action requires its own verification.
        /// </summary>
        public TimeSpan Validity =>
            TimeSpan.FromMinutes(
                RequiresVerificationPerAction
                    ? PER_ACTION_VERIFICATION_WINDOW_IN_MINUTES
                    : ValidityInMinutes
            );
    }
}
