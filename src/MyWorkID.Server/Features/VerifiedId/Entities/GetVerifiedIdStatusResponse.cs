namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// Response of the Verified ID status endpoint consumed by the frontend.
    /// </summary>
    /// <param name="Required"><c>true</c> if sensitive actions require a recent verification.</param>
    /// <param name="ValidityInMinutes">How long a verification counts as recent, as configured. <c>0</c> when every action requires its own verification.</param>
    /// <param name="RequiresVerificationPerAction"><c>true</c> if every sensitive action requires its own verification. A verification then unlocks a single action and has to be used within a short window.</param>
    /// <param name="IsVerified"><c>true</c> if the user has a verification that is still within the validity window. Always <c>false</c> when the requirement is disabled, because the verification state is not looked up in that case.</param>
    /// <param name="LastVerifiedAt">Timestamp of the last successful verification, if known.</param>
    /// <param name="VerifiedForAction">The action the last verification is bound to, if any. When every action requires its own verification, only this action is unlocked.</param>
    /// <param name="ValidUntil">Point in time until which the last verification counts as recent, if known.</param>
    /// <param name="ServerTime">The server's current time when the response was created. Lets the client evaluate <c>ValidUntil</c> without relying on its own, possibly skewed, clock.</param>
    public record GetVerifiedIdStatusResponse(
        bool Required,
        int ValidityInMinutes,
        bool RequiresVerificationPerAction,
        bool IsVerified,
        DateTimeOffset? LastVerifiedAt,
        VerifiedIdAction? VerifiedForAction,
        DateTimeOffset? ValidUntil,
        DateTimeOffset ServerTime
    );
}
