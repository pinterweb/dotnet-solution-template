namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Interface to validate the data from an instance <typeparam name="T">T</typeparam>
    /// </summary>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validates the instance, returning a result indicating success or failure
        /// .</summary>
        /// <param name="instance">The instance to validate.</param>
        Task<Result<Unit, Exception>> ValidateAsync(T instance, CancellationToken cancelToken);
    }
}
