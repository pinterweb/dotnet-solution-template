namespace BusinessApp.App
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using BusinessApp.Domain;

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

            foreach (var member in Result.MemberNames)
            {
                Data.Add(member, Result.ErrorMessage);
            }
        }

        public ValidationException(string memberName, string message, Exception inner = null)
            :this(new ValidationResult(message, new[] { memberName }), inner)
        {}

        public ValidationException(string message)
            :this(new ValidationResult(message, new[] { "" }))
        {}

        public ValidationResult Result { get; }
    }
}
