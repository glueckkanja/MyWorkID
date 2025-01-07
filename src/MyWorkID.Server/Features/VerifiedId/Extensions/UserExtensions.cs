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
    }
}
