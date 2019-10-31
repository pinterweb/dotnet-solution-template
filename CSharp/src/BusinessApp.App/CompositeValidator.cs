namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs multiple validators for one instance of {T}
    /// </summary>
    public class CompositeValidator<T> : IValidator<T>
    {
        private readonly IEnumerable<IValidator<T>> validators;

        public CompositeValidator(IEnumerable<IValidator<T>> validators)
        {
            this.validators = GuardAgainst.Null(validators, nameof(validators));
        }

        public void ValidateObject(T instance)
        {
            var errors = new List<ValidationResult>();

            foreach (var validator in validators)
            {
                try
                {
                    validator.ValidateObject(instance);
                }
                catch (ValidationException ex)
                {
                    errors.AddRange(ex.Results);
                }
            }

            if (errors.Any())
            {
                throw new ValidationException(
                    new CompositeValidationResult("Multiple validation errors occurred", errors)
                );
            }
        }
    }
}
