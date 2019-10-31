namespace BusinessApp.App
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Represents many validation exceptions
    /// </summary>
    public class CompositeValidationResult : ValidationResult, IEnumerable<ValidationResult>
    {
        public CompositeValidationResult(string errorMessage, IEnumerable<ValidationResult> results)
            : base(errorMessage, results.SelectMany(r => r.MemberNames))
        {
            Results = results;
        }

        public IEnumerable<ValidationResult> Results { get; }
        public IEnumerator<ValidationResult> GetEnumerator() => Results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
