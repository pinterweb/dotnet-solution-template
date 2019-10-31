namespace BusinessApp.App
{
    using System;

    /// <summary>
    /// Interface to validate the data from an instance <typeparam name="T">T</typeparam>
    /// </summary>
    public interface IValidator<T>
    {
        /// <summary>Validates the given instance.</summary>
        /// <param name="instance">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when the instance is a null reference.</exception>
        /// <exception cref="ValidationException">Thrown when the instance is invalid.</exception>
        void ValidateObject(T instance);
    }
}
