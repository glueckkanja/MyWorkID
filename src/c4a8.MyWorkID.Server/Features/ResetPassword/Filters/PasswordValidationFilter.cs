using c4a8.MyWorkID.Server.Features.ResetPassword.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace c4a8.MyWorkID.Server.Features.ResetPassword.Filters
{
    /// <summary>
    /// Validates the password in a password reset request.
    /// </summary>
    public class PasswordValidationFilter : IEndpointFilter
    {
        /// <summary>
        /// Invokes the filter to validate the password in the request.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context.</param>
        /// <param name="next">The next filter delegate.</param>
        /// <returns>The result of the validation or the next filter in the pipeline.</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var pwRequest = context.GetArgument<PasswordResetRequest>(0);
            var validationProblemDetails = new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
            };

            // Check if the new password is null or empty
            if (pwRequest.NewPassword == null || string.IsNullOrWhiteSpace(pwRequest.NewPassword))
            {
                validationProblemDetails.Errors.Add(nameof(pwRequest.NewPassword), [Strings.PASSWORD_VALIDATION_MISSING_ERROR]);
                return Results.ValidationProblem(validationProblemDetails.Errors);
            }

            // Check if the new password length is within the valid range
            if (pwRequest.NewPassword.Length is < 8 or > 255)
            {
                validationProblemDetails.Errors.Add(nameof(pwRequest.NewPassword), [Strings.PASSWORD_VALIDATION_LENGTH_ERROR]);
                return Results.ValidationProblem(validationProblemDetails.Errors);
            }

            int counter = 0;
            // Taken from https://learn.microsoft.com/en-us/entra/identity/authentication/concept-sspr-policy#microsoft-entra-password-policies
            List<string> patterns = [
                    @"[a-z]",
                        @"[A-Z]",
                        @"[0-9]",
                        @"[@#%\^&\*\-_\!\+=\[\]{}\|\\:',\.\?\/`~""\(\);<> ]"
                ];
            // Count the number of patterns matched in the new password
            counter += patterns.Count(p => Regex.IsMatch(pwRequest.NewPassword, p, RegexOptions.None, TimeSpan.FromMilliseconds(500)));
            // Check if at least 3 different character types are present
            if (counter < 3)
            {
                validationProblemDetails.Errors.Add(nameof(pwRequest.NewPassword), [Strings.PASSWORD_VALIDATION_SYMBOLS_ERROR]);
                return Results.ValidationProblem(validationProblemDetails.Errors);
            }

            return await next(context);
        }
    }
}