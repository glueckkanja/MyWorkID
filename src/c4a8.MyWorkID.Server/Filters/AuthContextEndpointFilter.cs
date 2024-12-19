using c4a8.MyWorkID.Server.Common;
using Microsoft.AspNetCore.Mvc;

namespace c4a8.MyWorkID.Server.Filters
{
    /// <summary>
    /// Abstract base class for endpoint filters that require authentication context.
    /// </summary>

    public abstract class AuthContextEndpointFilter : IEndpointFilter
    {
        private readonly AppFunctions _appFunction;
        private readonly IAuthContextService _authContextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthContextEndpointFilter"/> class.
        /// </summary>
        /// <param name="appFunction">The application function that requires authentication context.</param>
        /// <param name="authContextService">The authentication context service.</param>
        protected AuthContextEndpointFilter(AppFunctions appFunction, IAuthContextService authContextService)
        {
            _appFunction = appFunction;
            _authContextService = authContextService;
        }

        /// <summary>
        /// Invokes the endpoint filter asynchronously.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context.</param>
        /// <param name="next">The next delegate to invoke.</param>
        /// <returns>The result of the filter invocation.</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var httpContext = context.HttpContext;
            if (httpContext == null || httpContext.User == null || httpContext.User.Claims == null || !httpContext.User.Claims.Any())
            {
                return Results.Problem(new ProblemDetails
                {
                    Detail = "No user or claims provided.",
                    Status = StatusCodes.Status401Unauthorized,
                });
            }
            string? claimsChallenge = _authContextService.CheckForRequiredAuthContext(httpContext, _appFunction);
            if (!string.IsNullOrWhiteSpace(claimsChallenge))
            {
                string? missingAuthContextId = _authContextService.GetAuthContextId(_appFunction);
                if (string.IsNullOrWhiteSpace(missingAuthContextId))
                {
                    throw new InvalidOperationException($"Missing AuthContextId for {_appFunction}");
                }
                _authContextService.AddClaimsChallengeHeader(httpContext, missingAuthContextId!);
                var problemDetails = new ProblemDetails
                {
                    Detail = _authContextService.GetClaimsChallengeMessage(),
                    Status = StatusCodes.Status401Unauthorized,
                };
                return Results.Problem(problemDetails);
            }

            return await next(context);
        }
    }

    /// <summary>
    /// Endpoint filter for the GenerateTap function.
    /// </summary>
    class GenerateTapAuthContextEndpointFilter : AuthContextEndpointFilter
    {
        public GenerateTapAuthContextEndpointFilter(IAuthContextService authContextService) : base(AppFunctions.GenerateTap, authContextService) { }
    }

    /// <summary>
    /// Endpoint filter for the ResetPassword function.
    /// </summary>
    class ResetPasswordAuthContextEndpointFilter : AuthContextEndpointFilter
    {
        public ResetPasswordAuthContextEndpointFilter(IAuthContextService authContextService) : base(AppFunctions.ResetPassword, authContextService) { }
    }

    /// <summary>
    /// Endpoint filter for the DismissUserRisk function.
    /// </summary>
    class DismissUserRiskAuthContextEndpointFilter : AuthContextEndpointFilter
    {
        public DismissUserRiskAuthContextEndpointFilter(IAuthContextService authContextService) : base(AppFunctions.DismissUserRisk, authContextService) { }
    }
}