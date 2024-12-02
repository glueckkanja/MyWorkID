using c4a8.MyWorkID.Server.Common;
using Microsoft.AspNetCore.Mvc;

namespace c4a8.MyWorkID.Server.Filters
{
    public abstract class AuthContextEndpointFilter : IEndpointFilter
    {
        private readonly AppFunctions _appFunction;
        private readonly IAuthContextService _authContextService;

        protected AuthContextEndpointFilter(AppFunctions appFunction, IAuthContextService authContextService)
        {
            _appFunction = appFunction;
            _authContextService = authContextService;
        }

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

    class GenerateTapAuthContextEndpointFilter : AuthContextEndpointFilter
    {
        public GenerateTapAuthContextEndpointFilter(IAuthContextService authContextService) : base(AppFunctions.GenerateTap, authContextService) { }
    }

    class ResetPasswordAuthContextEndpointFilter : AuthContextEndpointFilter
    {
        public ResetPasswordAuthContextEndpointFilter(IAuthContextService authContextService) : base(AppFunctions.ResetPassword, authContextService) { }
    }

    class DismissUserRiskAuthContextEndpointFilter : AuthContextEndpointFilter
    {
        public DismissUserRiskAuthContextEndpointFilter(IAuthContextService authContextService) : base(AppFunctions.DismissUserRisk, authContextService) { }
    }
}