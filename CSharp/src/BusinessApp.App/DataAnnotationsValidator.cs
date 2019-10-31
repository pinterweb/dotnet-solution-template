namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;

    /// <summary>
    /// Runs validations for data annotations
    /// </summary>
    public class DataAnnotationsValidator<TCommand> : IValidator<TCommand>
    {
        [DebuggerStepThrough]
        void IValidator<TCommand>.ValidateObject(TCommand instance)
        {
            var context = new ValidationContext(instance);
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(instance, context, errors, true);

            if (!isValid)
            {
                throw new ValidationException(
                    new CompositeValidationResult("Multiple validation errors occurred", errors)
                );
            }
        }
    }
}
