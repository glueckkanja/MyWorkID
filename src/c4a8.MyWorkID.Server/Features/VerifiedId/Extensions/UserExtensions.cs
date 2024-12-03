namespace c4a8.MyWorkID.Server.Features.VerifiedId.Extensions
{
    public static class UserExtensions
    {
        public static string? GetUserId(this System.Security.Claims.ClaimsPrincipal user)
        {
            return user.FindFirst("userId")?.Value;
        }
    }
}
