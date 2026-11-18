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
        /// Marks the current verification of the user as used if every action requires its own verification
        /// and the current verification is bound to <paramref name="action"/>. Verifications that are not bound
        /// to an action (started from the standalone identity validation) stay untouched.
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
    }
}
