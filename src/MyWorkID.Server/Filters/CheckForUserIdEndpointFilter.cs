namespace MyWorkID.Server.Filters
{
    /// <summary>
    /// Endpoint filter that checks for the presence of a user ID in the user's claims.
    /// </summary>
    public class CheckForUserIdEndpointFilter : IEndpointFilter
    {
        /// <summary>
        /// Invokes the endpoint filter asynchronously.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context.</param>
        /// <param name="next">The next delegate to invoke.</param>
        /// <returns>The result of the filter invocation.</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var userId = context.HttpContext.User.FindFirst("userId");
            if (string.IsNullOrWhiteSpace(userId?.Value))
            {
                return Results.Unauthorized();
            }
            return await next(context);
        }
    }
}
