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
    }
}
