namespace MyWorkID.Server.Features.VerifiedId.Entities
{
    /// <summary>
    /// A verification that a request claimed for a single action. While the claim is stored in the custom security
    /// attribute, the verification counts as used and no other request can claim it.
    /// </summary>
    /// <param name="UserId">Object ID of the user the verification belongs to.</param>
    /// <param name="ClaimedValue">The value stored in the custom security attribute by this claim.</param>
    /// <param name="ReleasedValue">The value that restores the claimed verification if the action fails.</param>
    public record VerifiedIdVerificationClaim(
        string UserId,
        string ClaimedValue,
        string ReleasedValue
    );
}
