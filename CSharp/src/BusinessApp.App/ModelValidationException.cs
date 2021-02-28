namespace BusinessApp.App
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;

    [Serializable]
    public class ModelValidationException : Exception, IEnumerable<MemberValidationException>
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

            Data.Add("ValidationErrors", memberErrors.ToDictionary(e => e.MemberName, e => e.Errors));
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public IEnumerator<MemberValidationException> GetEnumerator() => memberErrors.GetEnumerator();
    }
}
