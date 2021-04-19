using System;

namespace BusinessApp.WebApi.ProblemDetails
{
    public interface IProblemDetailFactory
    {
        ProblemDetail Create(Exception error);
    }
}
