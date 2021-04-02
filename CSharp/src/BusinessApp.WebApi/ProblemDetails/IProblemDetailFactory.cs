namespace BusinessApp.WebApi.ProblemDetails
{
    using System;

    public interface IProblemDetailFactory
    {
        ProblemDetail Create(Exception error);
    }
}
