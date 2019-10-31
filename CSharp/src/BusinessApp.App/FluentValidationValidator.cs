namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs validations for any fluent validation rules
    /// </summary>
    public class FluentValidationValidator<TCommand> : IValidator<TCommand>
    {
        private readonly IEnumerable<FluentValidation.IValidator<TCommand>> validators;

        public FluentValidationValidator(IEnumerable<FluentValidation.IValidator<TCommand>> validators)
        {
            this.validators = GuardAgainst.Null(validators, nameof(validators));
        }

        public void ValidateObject(TCommand instance)
        {
            var errors = new List<ValidationResult>();

            foreach (var validator in validators)
            {
                var result = validator.Validate(instance);

                if (!result.IsValid)
                {
                    errors.AddRange(
                        result.Errors
                            .GroupBy(e => e.ErrorMessage)
                            .Select(g =>
                                new ValidationResult(g.Key, g.Select(v => v.PropertyName))
                            )
                    );
                }
            }
        }
    }
}
