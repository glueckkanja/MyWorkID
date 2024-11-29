using Microsoft.Identity.Web;

namespace c4a8.MyWorkID.Server.Filters
{
    public class CheckForUserIdEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var userId = context.HttpContext.User.GetObjectId();
            if (userId == null)
            {
                return Results.Unauthorized();
            }
            return await next(context);
        }
    }
}
