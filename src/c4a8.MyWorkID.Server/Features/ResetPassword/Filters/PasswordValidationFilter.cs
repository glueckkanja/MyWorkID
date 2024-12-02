using c4a8.MyWorkID.Server.Features.ResetPassword.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace c4a8.MyWorkID.Server.Features.ResetPassword.Filters
{
    public class PasswordValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var pwRequest = context.GetArgument<PasswordResetRequest>(0);
            var validationProblemDetails = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
            };

            if (pwRequest.NewPassword == null || string.IsNullOrWhiteSpace(pwRequest.NewPassword))
            {
                validationProblemDetails.Errors.Add(nameof(pwRequest.NewPassword), [Strings.PASSWORD_VALIDATION_MISSING_ERROR]);
                return Results.ValidationProblem(validationProblemDetails.Errors);
            }

            if (pwRequest.NewPassword.Length is < 8 or > 255)
            {
                validationProblemDetails.Errors.Add(nameof(pwRequest.NewPassword), [Strings.PASSWORD_VALIDATION_LENGTH_ERROR]);
                return Results.ValidationProblem(validationProblemDetails.Errors);
            }

            int counter = 0;
            // taken from https://learn.microsoft.com/en-us/entra/identity/authentication/concept-sspr-policy#microsoft-entra-password-policies
            List<string> patterns = new List<string> {
                    @"[a-z]",
                    @"[A-Z]",
                    @"[0-9]",
                    @"[@#%\^&\*\-_\!\+=\[\]{}\|\\:',\.\?\/`~""\(\);<> ]"
                };
            counter += patterns.Count(p => Regex.IsMatch(pwRequest.NewPassword, p));
            if (counter < 3)
            {
                validationProblemDetails.Errors.Add(nameof(pwRequest.NewPassword), [Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR]);
                return Results.ValidationProblem(validationProblemDetails.Errors);
            }

            return await next(context);
        }
    }
}