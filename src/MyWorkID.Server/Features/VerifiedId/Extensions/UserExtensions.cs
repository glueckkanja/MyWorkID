using MyWorkID.Server.Features.VerifiedId.Entities;

namespace MyWorkID.Server.Features.VerifiedId.Extensions
{
    /// <summary>
    /// Provides extension methods for user-related operations.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Retrieves the user ID from the claims principal.
        /// </summary>
        /// <param name="user">The claims principal representing the user.</param>
        /// <returns>The user ID if found; otherwise, null.</returns>
        public static string? GetUserId(this System.Security.Claims.ClaimsPrincipal user)
        {
            return user.FindFirst("userId")?.Value;
        }

        /// <summary>
        /// Retrieves the action the verification was started for from the Verified ID callback token.
        /// </summary>
        /// <param name="user">The claims principal of the Verified ID callback.</param>
        /// <returns>The action, or null if the verification was not started for a specific action.</returns>
        public static VerifiedIdAction? GetVerifiedIdAction(
            this System.Security.Claims.ClaimsPrincipal user
        )
        {
            string? value = user.FindFirst(Strings.VERIFIED_ID_ACTION_CLAIM)?.Value;
            return VerificationAttributeValue.TryParseAction(value, out VerifiedIdAction action)
                ? action
                : null;
        }
    }
}
