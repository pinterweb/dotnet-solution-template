using System;

namespace BusinessApp.WebApi.ProblemDetails
{
    /// <summary>
    /// Service to creat a <see cref="ProblemDetail" /> from an exception
    /// </summary>
    public interface IProblemDetailFactory
    {
        ProblemDetail Create(Exception exception);
    }
}
