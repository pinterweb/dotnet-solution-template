namespace BusinessApp.App
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
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
            Result = Guard.Against.Null(result).Expect(nameof(result));

            if (Result.MemberNames.Any())
            {
                foreach (var member in Result.MemberNames)
                {
                    Data.Add(member, Result.ErrorMessage);
                }
            }
            else
            {
                Data.Add("", Result.ErrorMessage);
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
