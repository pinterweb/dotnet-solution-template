namespace BusinessApp.App
{
    using System.ComponentModel.DataAnnotations;
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
        public Task ValidateAsync(T instance, CancellationToken cancellationToken)
        {
            var context = new ValidationContext(instance);
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(instance, context, errors, true);

            if (!isValid)
            {
                var members = errors.SelectMany(e => e.MemberNames).Distinct();

                if (errors.Any(e => !e.MemberNames.Any()))
                {
                    throw new BusinessAppAppException("All errors must have a member name. " +
                        "If the attribute does not support this, please create or extend the attribute");
                }

                var memberMsgs = (members.Any() ? members : new[] { "" })
                    .ToDictionary(
                        m => m,
                        m => errors.Where(e => e.MemberNames.Contains(m)).Select(e => e.ErrorMessage));

                throw new ModelValidationException(
                    "The model did not pass validation. See erros for more details",
                    memberMsgs.Select(kvp => new MemberValidationException(kvp.Key, kvp.Value)));
            }

            return Task.CompletedTask;
        }
    }
}
