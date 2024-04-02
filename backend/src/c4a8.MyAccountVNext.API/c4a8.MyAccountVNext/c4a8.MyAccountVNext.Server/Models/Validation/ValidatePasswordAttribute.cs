using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace c4a8.MyAccountVNext.Server.Models.Validation
{
    public class ValidatePasswordAttribute : ValidationAttribute
    {
        public ValidatePasswordAttribute()
        {
            const string defaultErrorMessage = "Error with password";
            ErrorMessage ??= defaultErrorMessage;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Password is required.");
            }
            var pwd = value as string;
            if (pwd.Length is < 8 or > 255)
            {
                return new ValidationResult("Password must be between 8 and 255 characters.");
            }
            int counter = 0;
            // taken from https://learn.microsoft.com/en-us/entra/identity/authentication/concept-sspr-policy#microsoft-entra-password-policies
            List<string> patterns = new List<string> {
                @"[a-z]",
                @"[A-Z]",
                @"[0-9]",
                @"[@#%\^&\*\-_\!\+=\[\]{}\|\\:',\.\?\/`~""\(\);<> ]"
            };
            foreach (string p in patterns)
            {
                if (Regex.IsMatch(pwd, p))
                {
                    counter++;
                }
            }

            if (counter < 3)
            {
                return new ValidationResult("Please use characters from at least 3 of these groups: lowercase, uppercase, digits, special symbols.");
            }
            return ValidationResult.Success;
        }
    }
}
