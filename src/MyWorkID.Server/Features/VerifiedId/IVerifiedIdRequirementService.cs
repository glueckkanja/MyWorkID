using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.Features.VerifiedId
{
    /// <summary>
    /// Determines whether sensitive actions require a recent Verified ID verification and looks up
    /// the verification state of a user.
    /// </summary>
    public interface IVerifiedIdRequirementService
    {
        /// <summary>
        /// <c>true</c> if sensitive actions require a recent verification.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// How long a successful verification counts as recent.
        /// </summary>
        TimeSpan Validity { get; }

        /// <summary>
        /// How long a successful verification counts as recent, in whole minutes.
        /// </summary>
        int ValidityInMinutes { get; }

        /// <summary>
        /// <c>true</c> if every sensitive action requires its own verification, because no validity window is configured.
        /// </summary>
        bool RequiresVerificationPerAction { get; }

        /// <summary>
        /// Reads the last verification timestamp of the user from the configured custom security attribute
        /// and evaluates it against the configured validity.
        /// </summary>
        /// <param name="userId">Object ID of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The verification state of the user.</returns>
        /// <exception cref="Exceptions.ConfigurationMissingException">Thrown when the target security attribute is not configured.</exception>
        Task<VerifiedIdVerificationState> GetVerificationStateAsync(
            string userId,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Marks the current verification of the user as used, so the next sensitive action requires a new
        /// verification. Overwrites the verification timestamp in the configured custom security attribute.
        /// </summary>
        /// <param name="userId">Object ID of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Exceptions.ConfigurationMissingException">Thrown when the target security attribute is not configured.</exception>
        Task MarkVerificationAsUsedAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Claims the current verification of the user for a single action, so concurrent requests cannot use the
        /// same verification. The claim marks the verification as used and is then read back: only the request
        /// whose claim is still stored after <see cref="VerifiedIdRequirementService.CLAIM_SETTLE_DELAY"/> wins. While
        /// the directory still returns the claimed verification, the claim is not readable yet and the attribute is
        /// read again (see <see cref="VerifiedIdRequirementService.CLAIM_READ_RETRY_INTERVAL"/>).
        /// </summary>
        /// <param name="userId">Object ID of the user.</param>
        /// <param name="verificationState">The verification that was checked before claiming it.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The claim if this request won it; otherwise <c>null</c>.</returns>
        /// <exception cref="Exceptions.ConfigurationMissingException">Thrown when the target security attribute is not configured.</exception>
        Task<VerifiedIdVerificationClaim?> TryClaimVerificationAsync(
            string userId,
            VerifiedIdVerificationState verificationState,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Restores the claimed verification after the action failed, unless the attribute was overwritten in the
        /// meantime (e.g. by a new verification). While the directory still returns the verification from before the
        /// claim, the attribute is read again like in <see cref="TryClaimVerificationAsync"/>.
        /// </summary>
        /// <param name="claim">The claim returned by <see cref="TryClaimVerificationAsync"/>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Exceptions.ConfigurationMissingException">Thrown when the target security attribute is not configured.</exception>
        Task ReleaseVerificationClaimAsync(
            VerifiedIdVerificationClaim claim,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Marks the current verification of the user as used if every action requires its own verification
        /// and the current verification is bound to <paramref name="action"/>. Verifications that are not bound
        /// to an action (started from the standalone identity validation) stay untouched. The stored value names
        /// the discarded verification, so a verification that is recorded concurrently and overwritten by the
        /// discard can restore itself (see <see cref="RestoreVerificationOverwrittenByDiscardAsync"/>).
        /// </summary>
        /// <param name="userId">Object ID of the user.</param>
        /// <param name="action">The action the verification to discard was started for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if a verification was discarded.</returns>
        /// <exception cref="Exceptions.ConfigurationMissingException">Thrown when the target security attribute is not configured.</exception>
        Task<bool> DiscardVerificationAsync(
            string userId,
            VerifiedIdAction action,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Restores a newly recorded verification if a concurrent discard of an earlier verification overwrote
        /// it. Graph has no conditional updates, so a discard that looked up the earlier verification before the
        /// new one was recorded overwrites the new one. Waits
        /// <see cref="VerifiedIdRequirementService.CLAIM_SETTLE_DELAY"/> before reading the attribute back. Does
        /// nothing unless every action requires its own verification.
        /// </summary>
        /// <param name="userId">Object ID of the user.</param>
        /// <param name="verifiedAt">When the new verification was completed.</param>
        /// <param name="action">The action the new verification is bound to, if any.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="Exceptions.ConfigurationMissingException">Thrown when the target security attribute is not configured.</exception>
        Task RestoreVerificationOverwrittenByDiscardAsync(
            string userId,
            DateTimeOffset verifiedAt,
            VerifiedIdAction? action,
            CancellationToken cancellationToken
        );
    }
}
