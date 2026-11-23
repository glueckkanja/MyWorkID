namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// A sensitive action a Verified ID verification can be bound to. When every action requires its own
    /// verification, a verification only unlocks the action it was started for.
    /// </summary>
    public enum VerifiedIdAction
    {
        DismissUserRisk,
        GenerateTemporaryAccessPass,
        ResetPassword,
    }
}
