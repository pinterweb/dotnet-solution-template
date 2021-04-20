using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Runs validations for any fluent validation rules
    /// </summary>
    public class FluentValidationValidator<T> : IValidator<T>
        where T : notnull
    {
        private readonly IEnumerable<FluentValidation.IValidator<T>> validators;

        public FluentValidationValidator(IEnumerable<FluentValidation.IValidator<T>> validators)
        {
            this.validators = validators.NotNull().Expect(nameof(validators));
        }

        public async Task<Result<Unit, Exception>> ValidateAsync(T instance,
            CancellationToken cancelToken)
        {
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(instance);

                if (!result.IsValid)
                {
                    return Result.Error(
                        new ModelValidationException(
                            "Model failed validation. See errors for more detials",
                            result.Errors
                                .GroupBy(e => e.PropertyName)
                                .Select(g => new MemberValidationException(g.Key, g.Select(v => v.ErrorMessage))))
                    );
                }
            }

            return Result.OK;
        }
    }
}
