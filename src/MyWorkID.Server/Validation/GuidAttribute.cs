using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class GuidAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string stringValue || !Guid.TryParse(stringValue, out _))
            {
                var memberName = validationContext.DisplayName ?? validationContext.MemberName;
                return new ValidationResult($"The field '{memberName}' must be a valid GUID.");
            }

            return ValidationResult.Success!;
        }
    }
}
