namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// Describes when a user last completed a Verified ID verification and whether that
    /// verification is still within the configured validity window.
    /// </summary>
    /// <param name="LastVerifiedAt">Timestamp of the last successful verification, or <c>null</c> if the user was never verified.</param>
    /// <param name="ValidUntil">Point in time until which the last verification counts as recent, or <c>null</c> if the user was never verified.</param>
    /// <param name="IsValid"><c>true</c> if a verification exists and has not expired yet.</param>
    public record VerifiedIdVerificationState(
        DateTimeOffset? LastVerifiedAt,
        DateTimeOffset? ValidUntil,
        bool IsValid
    )
    {
        /// <summary>
        /// State of a user that has never completed a verification.
        /// </summary>
        public static readonly VerifiedIdVerificationState NeverVerified = new(null, null, false);
    }
}
