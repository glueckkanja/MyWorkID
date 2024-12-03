﻿namespace c4a8.MyWorkID.Server.Filters
{
    public class CheckForUserIdEndpointFilter : IEndpointFilter
    {
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
