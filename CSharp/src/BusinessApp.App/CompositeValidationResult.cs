using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BusinessApp.App
{
    public class CompositeValidationResult : ValidationResult, IEnumerable<ValidationResult>
    {
        public CompositeValidationResult(string instanceName, string errorMessage, IEnumerable<ValidationResult> results)
            : base(errorMessage, results.SelectMany(r => r.MemberNames).Select(m => instanceName != null ? $"{instanceName}.{m}" : m))
        {
            this.Results = results;
        }

        public IEnumerable<ValidationResult> Results { get; }
        public IEnumerator<ValidationResult> GetEnumerator() => this.Results.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
