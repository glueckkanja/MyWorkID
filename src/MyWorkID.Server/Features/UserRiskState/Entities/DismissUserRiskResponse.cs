namespace MyWorkID.Server.Features.UserRiskState.Entities
{
    /// <summary>
    /// Represents the result of dismissing the user risk state.
    /// </summary>
    /// <param name="SessionsRevoked">
    /// Whether all sign-in sessions of the user were revoked. This is <c>false</c> when the risk was dismissed
    /// but the backend is not permitted to revoke sign-in sessions.
    /// </param>
    public record DismissUserRiskResponse(bool SessionsRevoked);
}
