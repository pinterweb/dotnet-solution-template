using System;
using System.Collections.Generic;
using System.Linq;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    [Serializable]
    public class MemberValidationException : BusinessAppException
    {
        public MemberValidationException(string memberName, IEnumerable<string> errors)
            : base($"'{memberName}' failed validation. See errors for more details")
        {
            MemberName = memberName.NotEmpty().Expect(nameof(memberName));
            Errors = errors.NotEmpty().Expect(nameof(errors)).ToList();
        }

        public string MemberName { get; }
        public IReadOnlyCollection<string> Errors { get; }
    }
}
