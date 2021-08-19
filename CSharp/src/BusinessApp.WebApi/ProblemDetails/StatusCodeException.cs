using System;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi.ProblemDetails
{
    public abstract class StatusCodeException : Exception
    {
        public StatusCodeException(int statusCode, string message)
            : base(message)
        {
            _ = message.NotEmpty().Expect(nameof(message));

            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}
