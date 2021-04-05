namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public class CompositeProblemDetail : ProblemDetail, IEnumerable<ProblemDetail>
    {
        public CompositeProblemDetail(IEnumerable<ProblemDetail> problems, Uri? type = null)
            : base(StatusCodes.Status207MultiStatus, type)
        {
            Responses = problems.NotEmpty().Expect(nameof(problems));

           this[nameof(Responses)]  = Responses;
        }

        /// <summary>
        /// All responses in one array. Can have a mix of ok statuses with bad statuses
        /// </summary>
        public IEnumerable<ProblemDetail> Responses { get; }

        IEnumerator<ProblemDetail> IEnumerable<ProblemDetail>.GetEnumerator() => Responses.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
