using Microsoft.Identity.Web;
using MyWorkID.Server.Features.VerifiedId;
using MyWorkID.Server.Common;

namespace MyWorkID.Server.Filters
{
    /// <summary>
    /// Endpoint filter that checks for recent VerifiedID validation based on the security attribute.
    /// Only applies to users who have the RequireRecentVerifiedId role.
    /// </summary>
    public class RequireRecentVerifiedIdEndpointFilter : IEndpointFilter
    {
        private readonly IVerifiedIdService _verifiedIdService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireRecentVerifiedIdEndpointFilter"/> class.
        /// </summary>
        /// <param name="verifiedIdService">The VerifiedID service.</param>
        public RequireRecentVerifiedIdEndpointFilter(IVerifiedIdService verifiedIdService)
        {
            _verifiedIdService = verifiedIdService;
        }

        /// <summary>
        /// Invokes the endpoint filter asynchronously.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context.</param>
        /// <param name="next">The next delegate to invoke.</param>
        /// <returns>The result of the filter invocation.</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var userId = context.HttpContext.User.GetObjectId();
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // Only check for recent VerifiedID if the user has the RequireRecentVerifiedId role
            if (!context.HttpContext.User.IsInRole(Strings.REQUIRE_RECENT_VERIFIED_ID_ROLE))
            {
                // User doesn't require recent VerifiedID, proceed normally
                return await next(context);
            }

            var hasRecentVerifiedId = await _verifiedIdService.HasRecentVerifiedId(userId, context.HttpContext.RequestAborted);
            if (!hasRecentVerifiedId)
            {
                return Results.Problem(
                    detail: "Recent VerifiedID validation required to perform this action",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Recent VerifiedID Required");
            }

            return await next(context);
        }
    }
}