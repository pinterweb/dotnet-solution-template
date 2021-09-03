using System;
using BusinessApp.Kernel;
using BusinessApp.WebApi.ProblemDetails;
using System.Linq;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Fixes an issue with newtonsoft serializing the IDictionary extension of
    /// the <see cref="CompositeProblemDetail.Responses" />. It gets serialized
    /// as a dictionary [ Key, Val ], but it is a string
    /// </summary>
    public class NewtonsoftProblemDetailFactory : IProblemDetailFactory
    {
        private readonly IProblemDetailFactory inner;

        public NewtonsoftProblemDetailFactory(IProblemDetailFactory inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public ProblemDetail Create(Exception exception)
        {
            var problem = inner.Create(exception);

            if (problem is CompositeProblemDetail c)
            {
                problem["Responses"] = c.Responses.Select(i => i.ToDictionary(k => k.Key, v => v.Value));
            }

            return problem;
        }
    }
}
