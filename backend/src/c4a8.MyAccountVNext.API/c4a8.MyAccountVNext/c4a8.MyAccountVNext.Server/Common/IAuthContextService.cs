namespace c4a8.MyAccountVNext.Server.Common
{
    public interface IAuthContextService
    {
        string? GetAuthContextId(AppFunctions appFunction);
        string? CheckForRequiredAuthContext(HttpContext context, AppFunctions appFunction);
        void AddClaimsChallengeHeader(HttpContext httpContext, string authContextId);
        string GetClaimsChallengeMessage();
    }
}
