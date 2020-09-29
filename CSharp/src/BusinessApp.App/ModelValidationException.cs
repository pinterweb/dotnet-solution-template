namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    [Serializable]
    public class ModelValidationException : Exception, IEnumerable<MemberValidationException>
    {
        private readonly IEnumerable<MemberValidationException> memberErrors;

        public ModelValidationException(string message, IEnumerable<MemberValidationException> memberErrors)
            :base(message)
        {
            this.memberErrors = Guard.Against.Empty(memberErrors).Expect(nameof(memberErrors));

            foreach (var error in memberErrors)
            {
                Data.Add(error.MemberName, error.Errors);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public IEnumerator<MemberValidationException> GetEnumerator() => memberErrors.GetEnumerator();
    }
}
