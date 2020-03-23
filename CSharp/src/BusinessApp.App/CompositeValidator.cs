namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs multiple validators for one instance of <typeparam name="T">T</typeparam>
    /// </summary>
    public class CompositeValidator<T> : IValidator<T>
    {
        private readonly IEnumerable<IValidator<T>> validators;

        public CompositeValidator(IEnumerable<IValidator<T>> validators)
        {
            this.validators = GuardAgainst.Null(validators, nameof(validators));
        }

        public async Task ValidateAsync(T instance)
        {
            var errors = new List<ValidationException>();

            foreach (var validator in validators)
            {
                try
                {
                    await validator.ValidateAsync(instance);
                }
                catch (ValidationException ex)
                {
                    errors.Add(ex);
                }
            }

            if (errors.Any())
            {
                if (errors.Count == 1)
                {
                    throw errors.First();
                }
                else
                {
                    throw new AggregateException(errors);
                }
            }
        }
    }
}
