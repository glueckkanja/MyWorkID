namespace c4a8.MyAccountVNext.API.Services
{
    public interface IAuthContextService
    {
        string? GetAuthContextId(AppFunctions appFunction);
        string? CheckForRequiredAuthContext(HttpContext context, AppFunctions appFunction);
        Task AddClaimsChallengeHeader(HttpContext httpContext, string authContextId);
        string GetClaimsChallengeMessage();
    }
}
