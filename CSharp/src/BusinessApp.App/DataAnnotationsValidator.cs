using System.ComponentModel.DataAnnotations;

namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs validations for data annotations
    /// </summary>
    public class DataAnnotationsValidator<T> : IValidator<T>
    {
        Task IValidator<T>.ValidateAsync(T instance, CancellationToken cancellationToken)
        {
            var context = new ValidationContext(instance);
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(instance, context, errors, true);

            if (!isValid)
            {
                if (errors.Count == 1)
                {
                    throw new ValidationException(errors[0]);
                }
                else
                {
                    throw new AggregateException(errors.Select(e => new ValidationException(e)));
                }
            }

            return Task.CompletedTask;
        }
    }
}
