namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to validate the data from an instance <typeparam name="T">T</typeparam>
    /// </summary>
    public interface IValidator<T>
    {
        /// <summary>Validates the given instance.</summary>
        /// <param name="instance">The instance to validate.</param>
        Task ValidateAsync(T instance, CancellationToken cancellationToken);
    }
}
