using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Validation exception describing all model errors
    /// </summary>
    public class ModelValidationException : BusinessAppException, IEnumerable<MemberValidationException>
    {
        public const string ValidationKey = "ValidationErrors";
        private readonly IEnumerable<MemberValidationException> memberErrors;

        public ModelValidationException(string modelMessage)
            : base(modelMessage) => memberErrors = new List<MemberValidationException>();

        public ModelValidationException(string message, IEnumerable<MemberValidationException> memberErrors)
            : base(message)
        {
            this.memberErrors = memberErrors.NotEmpty().Expect(nameof(memberErrors));
            Data.Add(ValidationKey, memberErrors.ToDictionary(err => err.MemberName, err => err.Errors));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<MemberValidationException> GetEnumerator() => memberErrors.GetEnumerator();
    }
}
