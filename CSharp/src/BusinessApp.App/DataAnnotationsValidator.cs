using System.ComponentModel.DataAnnotations;

namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs validations for data annotations
    /// </summary>
    public class DataAnnotationsValidator<TCommand> : IValidator<TCommand>
    {
        Task IValidator<TCommand>.ValidateAsync(TCommand instance)
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
