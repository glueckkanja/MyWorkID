namespace c4a8.MyWorkID.Server.Common
{
    /// <summary>
    /// Provides methods for handling authentication context.
    /// </summary>
    public interface IAuthContextService
    {
        /// <summary>
        /// Gets the authentication context ID for a given app function.
        /// </summary>
        /// <param name="appFunction">The app function.</param>
        /// <returns>The authentication context ID.</returns>
        string? GetAuthContextId(AppFunctions appFunction);

        /// <summary>
        /// Checks if the required authentication context is present in the claims.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="appFunction">The app function.</param>
        /// <returns>A claims challenge if the required context is missing; otherwise, an empty string.</returns>
        string? CheckForRequiredAuthContext(HttpContext context, AppFunctions appFunction);

        /// <summary>
        /// Adds headers to the response to challenge the client to provide the required claims.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="authContextId">The authentication context ID required for authorization.</param>
        void AddClaimsChallengeHeader(HttpContext httpContext, string authContextId);

        /// <summary>
        /// Gets the claims challenge message.
        /// </summary>
        /// <returns>The claims challenge message.</returns>
        string GetClaimsChallengeMessage();
    }
}
