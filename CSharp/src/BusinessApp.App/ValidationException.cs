namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Aggregates all validations results into one exception
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException(IEnumerable<ValidationResult> results)
            :base("Multiple Validation Errors Occurred. Please see the inner errors for more details")
        {
            Results = GuardAgainst.Null(results, nameof(results));
        }

        public ValidationException(string message)
            :base(message)
        {
            Results = new[] { new ValidationResult(message) };
        }

        public IEnumerable<ValidationResult> Results { get; }

        /// <summary>
        /// Converts the <see cref="Results"/> into a key, value pair
        /// of property name and validation messages, rather than one message and many
        /// member names.
        /// </summary>
        public override IDictionary Data
        {
            get => Results.SelectMany(r => r.MemberNames)
                .ToDictionary(
                    member => member,
                    member => Results
                        .Where(r => r.MemberNames.Contains(member))
                        .Select(r => r.ErrorMessage));
        }
    }
}
