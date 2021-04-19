using System;
using System.Collections;
using System.Collections.Generic;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    [Serializable]
    public class ModelValidationException : BusinessAppException, IEnumerable<MemberValidationException>
    {
        private readonly IEnumerable<MemberValidationException> memberErrors;

        public ModelValidationException(string modelMessage)
            :base(modelMessage)
        {
            this.memberErrors = new List<MemberValidationException>();
        }

        public ModelValidationException(string message, IEnumerable<MemberValidationException> memberErrors)
            :base(message)
        {
            this.memberErrors = memberErrors.NotEmpty().Expect(nameof(memberErrors));
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public IEnumerator<MemberValidationException> GetEnumerator() => memberErrors.GetEnumerator();
    }
}
