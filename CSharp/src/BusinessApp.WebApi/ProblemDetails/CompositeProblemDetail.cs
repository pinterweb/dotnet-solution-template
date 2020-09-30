namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public class CompositeProblemDetail : ProblemDetail, IEnumerable<ProblemDetail>
    {
        public CompositeProblemDetail(IEnumerable<ProblemDetail> problems, Uri type = null)
            : base(StatusCodes.Status207MultiStatus, type)
        {
            Problems = Guard.Against.Empty(problems).Expect(nameof(problems));
        }

        public IEnumerable<ProblemDetail> Problems { get; set; }

        IEnumerator<ProblemDetail> IEnumerable<ProblemDetail>.GetEnumerator() => Problems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
