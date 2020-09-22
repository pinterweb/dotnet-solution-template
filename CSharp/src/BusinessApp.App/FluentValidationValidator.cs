namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs validations for any fluent validation rules
    /// </summary>
    public class FluentValidationValidator<T> : IValidator<T>
    {
        private readonly IEnumerable<FluentValidation.IValidator<T>> validators;

        public FluentValidationValidator(IEnumerable<FluentValidation.IValidator<T>> validators)
        {
            this.validators = Guard.Against.Null(validators).Expect(nameof(validators));
        }

        public async Task ValidateAsync(T instance, CancellationToken cancellationToken)
        {
            var errors = new List<ValidationResult>();

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(instance);

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
