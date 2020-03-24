namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Aggregates all validations results into one exception
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException(ValidationResult result, Exception inner = null)
            :base(result.ErrorMessage, inner)
        {
            Result = GuardAgainst.Null(result, nameof(result));
        }

        public ValidationException(string memberName, string message, Exception inner = null)
            :base(message, inner)
        {
            Result = new ValidationResult(message, new[] { memberName });
        }

        public ValidationException(string message)
            :base(message)
        {
            Result = new ValidationResult(message, new[] { "" });
        }

        public ValidationResult Result { get; }

        /// <summary>
        /// Converts the <see cref="Result"/> into a key, value pair
        /// of property name and validation messages
        /// </summary>
        public override IDictionary Data
        {
            get => Result.MemberNames
                .ToDictionary(
                    member => member,
                    member => Result.ErrorMessage);
        }
    }
}
