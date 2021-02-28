namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;

    [Serializable]
    public class MemberValidationException : Exception
    {
        public MemberValidationException(string memberName, IEnumerable<string> errors)
            : base($"'{memberName}' failed validation. See errors for more details")
        {
            MemberName = memberName.NotEmpty().Expect(nameof(memberName));
            Errors = errors.NotEmpty().Expect(nameof(errors)).ToList();
            Data.Add(memberName, Errors);
        }

        public string MemberName { get; }
        public IReadOnlyCollection<string> Errors { get; }
    }
}
