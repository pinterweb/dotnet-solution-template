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
            MemberName = Guard.Against.Empty(memberName).Expect(nameof(memberName));
            Errors = Guard.Against.Empty(errors).Expect(nameof(errors)).ToList();
            Data.Add(memberName, Errors);
        }

        public string MemberName { get; }
        public IReadOnlyCollection<string> Errors { get; }
    }
}
