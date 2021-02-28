using System;
using BusinessApp.Domain;

namespace BusinessApp.WebApi.ProblemDetails
{
    /// <summary>
    /// Provides a lazy creation service, since many times this may not be needed,
    /// especially if it is in the catch part of a try/catch
    /// </summary>
    public class LazyProblemDetailFactoryProxy : IProblemDetailFactory
    {
        private readonly Lazy<IProblemDetailFactory> inner;

        public LazyProblemDetailFactoryProxy(Lazy<IProblemDetailFactory> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public ProblemDetail Create(Exception error = null)
        {
            return this.inner.Value.Create(error);
        }
    }
}
