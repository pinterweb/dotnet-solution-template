using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Validates an enitire object, used with Data Annotations
    /// </summary>
    public class ValidateObjectAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var context = new ValidationContext(value, null, null);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(value, context, results, validateAllProperties: true);

            return isValid
                ? ValidationResult.Success
                : new CompositeValidationResult(
                    validationContext.DisplayName,
                    string.Format(CultureInfo.InvariantCulture, "Validation for {0} failed!", validationContext.DisplayName),
                    results);
        }
    }
}
