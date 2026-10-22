namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// Response of the Verified ID status endpoint consumed by the frontend.
    /// </summary>
    /// <param name="Required"><c>true</c> if sensitive actions require a recent verification.</param>
    /// <param name="ValidityInMinutes">How long a verification counts as recent.</param>
    /// <param name="IsVerified"><c>true</c> if the user has a verification that is still within the validity window. Always <c>false</c> when the requirement is disabled, because the verification state is not looked up in that case.</param>
    /// <param name="LastVerifiedAt">Timestamp of the last successful verification, if known.</param>
    /// <param name="ValidUntil">Point in time until which the last verification counts as recent, if known.</param>
    public record GetVerifiedIdStatusResponse(
        bool Required,
        int ValidityInMinutes,
        bool IsVerified,
        DateTimeOffset? LastVerifiedAt,
        DateTimeOffset? ValidUntil
    );
}
