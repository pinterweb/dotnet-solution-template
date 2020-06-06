namespace BusinessApp.App
{
    using System.Collections.Generic;
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
            foreach (var validator in validators)
            {
                await validator.ValidateAsync(instance);
            }
        }
    }
}
