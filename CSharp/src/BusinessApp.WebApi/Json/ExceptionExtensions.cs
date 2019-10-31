namespace BusinessApp.WebApi.Json
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public static class ExceptionExtensions
    {
        /// <summary>
        /// Allows translating exceptions to HttpResponseExceptions.
        /// </summary>
        public static ResponseProblemBody MapToWebResponse(this JsonException exception, HttpContext context)
        {
            switch (exception)
            {
                // The supplied model (command or query) can't be deserialized.
                case JsonException _:
                    context.Response.StatusCode = 400;
                    return new ResponseProblemBody(context.Response.StatusCode,
                        "Data is not in the correct format",
                        context.CreateProblemUri("invalid-data-format")
                    )
                    {
                        Detail = exception.Message,
                    };
                default:
                    return ((Exception)exception).MapToWebResponse(context);
            }
        }
    }
}
