using System.ComponentModel.DataAnnotations;

namespace MyWorkID.Server.Validation
{
    public static class DataAnnotationsValidator
    {
        public static IList<ValidationResult> Validate(object obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            Validator.TryValidateObject(obj, context, results, validateAllProperties: true);
            return results;
        }
    }
}
