namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public class ProblemDetailFactoryHttpDecorator : IProblemDetailFactory
    {
        private readonly IProblemDetailFactory inner;
        private readonly IHttpContextAccessor accessor;

        public ProblemDetailFactoryHttpDecorator(IProblemDetailFactory inner, IHttpContextAccessor accessor)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.accessor = accessor.NotNull().Expect(nameof(accessor));
        }

        public ProblemDetail Create(IFormattable error)
        {
            if (accessor.HttpContext == null)
            {
                throw new BusinessAppWebApiException("Cannot access decorate the `ProblemDetail`" +
                    "with Http specifics because we are not in a http context");

            }

            var problem = inner.Create(error);

            accessor.HttpContext.Response.StatusCode =
                problem.StatusCode ?? StatusCodes.Status500InternalServerError;

            return problem;
        }
    }
}
