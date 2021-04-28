using System;

namespace BusinessApp.Infrastructure.WebApi.ProblemDetails
{
    public interface IProblemDetailFactory
    {
        ProblemDetail Create(Exception exception);
    }
}
