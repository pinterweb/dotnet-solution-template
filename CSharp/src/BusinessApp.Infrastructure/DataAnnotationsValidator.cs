using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Runs validations for data annotations
    /// </summary>
    public class DataAnnotationsValidator<T> : IValidator<T>
        where T : notnull
    {
        public Task<Result<Unit, Exception>> ValidateAsync(T instance, CancellationToken cancelToken)
        {
            var context = new ValidationContext(instance);
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(instance, context, errors, true);

            if (!isValid)
            {
                var members = errors.SelectMany(e => e.MemberNames).Distinct();

                if (errors.Any(e => !e.MemberNames.Any()))
                {
                    throw new BusinessAppException("All errors must have a member name. " +
                        "If the attribute does not support this, please create or extend the attribute");
                }

                if (errors.Any(e => e.ErrorMessage == null))
                {
                    throw new BusinessAppException("All errors must have an error message.");
                }

                var memberMsgs = (members.Any() ? members : new[] { "" })
                    .ToDictionary(
                        m => m,
                        m => errors.Where(e => e.MemberNames.Contains(m)).Select(e => e.ErrorMessage!));

                return Task.FromResult(
                    Result.Error(
                        new ModelValidationException(
                            "The model did not pass validation. See erros for more details",
                            memberMsgs.Select(kvp => new MemberValidationException(kvp.Key, kvp.Value)))
                    )
                );
            }

            return Task.FromResult(Result.OK);
        }
    }
}
