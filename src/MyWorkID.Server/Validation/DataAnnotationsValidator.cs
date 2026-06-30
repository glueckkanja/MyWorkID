using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Validation
{
    public static class DataAnnotationsValidator
    {
        public static IList<ValidationResult> Validate(object obj)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            ValidationContext context = new ValidationContext(
                obj,
                serviceProvider: null,
                items: null
            );
            Validator.TryValidateObject(obj, context, results, validateAllProperties: true);
            return results;
        }
    }
}
