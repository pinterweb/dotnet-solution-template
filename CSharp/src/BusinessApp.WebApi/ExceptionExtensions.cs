using System.ComponentModel.DataAnnotations;

namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public static class ExceptionExtensions
    {
        /// <summary>
        /// Allows translating exceptions to <see cref="ResponseProblemBody />
        /// </summary>
        public static ResponseProblemBody MapToWebResponse(this Exception exception, HttpContext context)
        {
            string errorType = "server-error";
            string title = "Server Error";
            IDictionary<string, IEnumerable<string>> errors =
                new Dictionary<string, IEnumerable<string>>();

            switch (exception)
            {
                // The supplied model isn't valid.
                case NotImplementedException _:
                    context.Response.StatusCode = 501;
                    errorType = "not-supported";
                    title = "Function not supported";
                    break;
                case BadStateException _:
                case ValidationException _:
                    context.Response.StatusCode = 400;
                    errorType = "invalid-model";
                    title = "Invalid Model";
                    errors = exception.Data.Keys.Cast<string>()
                        .ToDictionary(key => key, key => (IEnumerable<string>)exception.Data[key]);
                    break;
                case EntityNotFoundException _:
                case ResourceNotFoundException _:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorType = "not-found";
                    title = "Resource not found";
                    break;
                // The current user doesn't have the proper rights to execute the requested
                case SecurityException _:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorType = "insufficient-privileges";
                    title = "Insufficient privileges";
                    break;
                case FormatException _:
                    context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                    errorType = "malformed-data";
                    title = "Bad Data";
                    break;
                case DBConcurrencyException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorType = "conflict";
                    title = ex.Message;
                    break;
                case NotSupportedException ex:
                    errorType = "not-supported";
                    title = "The operation is not supported";
                    break;
                case TaskCanceledException ex:
                    context.Response.StatusCode = 400;
                    errorType = "cancelled";
                    title = ex.Message;
                    break;
                case AggregateException manyExceptions:
                    foreach(var ex in manyExceptions.Flatten().InnerExceptions)
                    {
                        var response = ex.MapToWebResponse(context);

                        if (response.Status != (int)HttpStatusCode.InternalServerError)
                        {
                            return response;
                        }
                    }
                    break;
                default:
                    if (new HttpResponseMessage((HttpStatusCode)context.Response.StatusCode).IsSuccessStatusCode)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    break;
            }

            return new ResponseProblemBody(context.Response.StatusCode,
                title,
                context.CreateProblemUri(errorType)
            )
            {
                Detail = exception.Message,
                Errors = errors
            };
        }

        public static ValidationException ToException(this ResponseProblemBody problem)
        {
            return new ValidationException(
                new CompositeValidationResult(problem.Detail,
                problem.Errors.SelectMany(e => e.Value.Select(v => new ValidationResult(v, new[] { e.Key })))));
        }
    }
}
