namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;
    using SimpleInjector;

    public static class ExceptionExtensions
    {
        /// <summary>
        /// Allows translating exceptions to <see cref="ResponseProblemBody" />
        /// </summary>
        public static ResponseProblemBody MapToWebResponse(this Exception exception, HttpContext context)
        {
            string errorType = "server-error";
            string title = "Server Error";
            string detail = null;
            var errors = new Dictionary<string, IEnumerable<string>>();

            switch (exception)
            {
                // The supplied model isn't valid.
                case BadStateException _:
                    context.Response.StatusCode = 400;
                    errorType = "invalid-model";
                    title = "Invalid Model";
                    break;
                case ValidationException ve:
                    context.Response.StatusCode = 400;
                    errorType = "invalid-data";
                    title = "Invalid Data";
                    detail = "Your data was not accepted because it is not valid. Please fix the errors";
                    errors = ve.Result.MemberNames
                        .ToDictionary(
                        member => member,
                        member => (IEnumerable<string>)new[] { ve.Result.ErrorMessage });
                    break;
                case ActivationException _:
                case ResourceNotFoundException _:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorType = "not-found";
                    title = "Resource not found";
                    break;
                case SecurityResourceException sre:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorType = "insufficient-privileges";
                    title = "Insufficient privileges";
                    detail = "";
                    errors = new Dictionary<string, IEnumerable<string>>
                    {
                        { sre.ResourceName, new[] { sre.Message } }
                    };
                    break;
                case SecurityException se:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorType = "insufficient-privileges";
                    title = "Insufficient privileges";
                    detail = se.Message;
                    break;
                case DBConcurrencyException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorType = "conflict";
                    title = "There was a conflict while updating your data.";
                    detail = $"{title} Please try again. " +
                        "If you continue to see this error please contact support.";
                    break;
                case NotImplementedException _:
                case NotSupportedException _:
                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorType = "not-supported";
                    title = "The operation is not supported.";
                    detail = "";
                    break;
                case TaskCanceledException ex:
                    context.Response.StatusCode = 400;
                    errorType = "canceled";
                    title = "The request has been canceled.";
                    detail = "";
                    break;
                case InvalidOperationException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorType = "not-supported";
                    title = "The operation is not supported because of bad data.";
                    detail = ex.Message;
                    break;
                case AggregateException manyExceptions:
                    var responses = new List<ResponseProblemBody>();

                    foreach (var ex in manyExceptions.Flatten().InnerExceptions)
                    {
                        responses.Add(ex.MapToWebResponse(context));
                    }

                    return new ResponseProblemBody(context.Response.StatusCode,
                        "Multiple Errors Occurred. Please see the errros.",
                        context.CreateProblemUri("multiple-errors"))
                    {
                        Detail = "",
                        Errors = responses.Aggregate(
                            new Dictionary<string, IEnumerable<string>>(),
                            (accu, problem) =>
                            {
                                if (!problem.Errors.Any())
                                {
                                    AddProblemErrors(accu,
                                        "",
                                        new[]
                                        {
                                            string.IsNullOrWhiteSpace(problem.Detail) ?
                                            problem.Title :
                                            problem.Detail
                                        });
                                }
                                else
                                {
                                    foreach (var error in problem.Errors)
                                    {
                                        AddProblemErrors(accu, error.Key, error.Value);
                                    }
                                }

                                return accu;
                            })
                    };
                default:
                    if (context.Response.IsSuccess())
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    errorType = "unexpected";
                    title = "An unexpected error has occurred in the system.";
                    detail = $"{title} The problem has been logged. Please contact support " +
                        "if this issue persists without our acknowledgment.";
                    break;
            }

            return new ResponseProblemBody(context.Response.StatusCode,
                title,
                context.CreateProblemUri(errorType)
            )
            {
                Detail = detail ?? exception.Message,
                Errors = errors
            };
        }

        private static void AddProblemErrors(IDictionary<string, IEnumerable<string>> accu, string key, IEnumerable<string> value)
        {
            if (!accu.TryGetValue(key, out IEnumerable<string> messages))
            {
                accu.Add(key, value);
            }
            else
            {
                accu[key] = messages.Concat(value).ToArray();
            }
        }
    }
}
