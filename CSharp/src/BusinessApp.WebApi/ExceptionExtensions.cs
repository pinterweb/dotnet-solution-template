namespace BusinessApp.WebApi
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Text.RegularExpressions;
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
            var errors = ToResponseProblemErrors(exception.Data);

            switch (exception)
            {
                case FormatException fe:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorType = "invalid-request";
                    detail = fe.Message
                        .Replace("Byte", "byte")
                        .Replace("Int16", "short")
                        .Replace("Int32", "int")
                        .Replace("Int64", "long")
                        .Replace("Double", "decimal")
                        .Replace("Decimal", "decimal")
                        .Replace("DateTime", "date")
                        .Replace("Single", "number")
                        .Replace("Boolean", "bool");
                    title = "Invalid Request";
                    break;
                case ArgumentException ae when ae.InnerException is FormatException:
                    var response =  new FormatException(Regex.Replace(ae.Message, " \\(.*\\)", ""))
                        .MapToWebResponse(context);
                    response.Errors = errors;

                    return response;
                case ModelValidationException ve:
                case BadStateException _:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorType = "invalid-data";
                    detail = "Your data was not accepted because it is not valid. Please fix the errors";
                    title = "Invalid Data";
                    break;
                case ActivationException _:
                case EntityNotFoundException _:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorType = "not-found";
                    title = "Resource not found";
                    detail = $"No resource was found at {context.Request.Path}";
                    break;
                case SecurityResourceException sre:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorType = "insufficient-privileges";
                    title = "Insufficient privileges";
                    detail = "";
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
                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorType = "not-supported";
                    title = "The operation is not supported.";
                    detail = "";
                    break;
                case NotSupportedException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    errorType = "not-supported";
                    title = "The operation is not supported.";
                    detail = ex.Message;
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
                case CommunicationException ex:
                    context.Response.StatusCode = (int)HttpStatusCode.FailedDependency;
                    errorType = "dependent-error";
                    title = "Communication Error";
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
                                if (problem.Errors == null)
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
                case Exception ex when ex.InnerException != null:
                    return ex.InnerException.MapToWebResponse(context);
                default:
                    errors = null;
                    if (context.Response.IsSuccess())
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                    errorType = "unexpected";
                    title = "An unexpected error has occurred in the system.";
#if !DEBUG
                    detail = $"{title} The problem has been logged. Please contact support " +
                        "if this issue persists without our acknowledgment.";
#else
                    detail = exception.Message;
#endif
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

        private static IDictionary<string, IEnumerable<string>> ToResponseProblemErrors(IDictionary data)
        {
            return data.Keys.Count == 0 ? null : data.Cast<DictionaryEntry>()
                         .Where(de => de.Key is string && (de.Value is string || de.Value is IEnumerable<string>))
                         .ToDictionary(
                             de => (string)de.Key,
                             de =>
                             {
                                return de.Value switch
                                {
                                    string strVal => (IEnumerable<string>)new[] { strVal },
                                    IEnumerable<string> ieVal => ieVal
                                };
                             });
        }
    }
}
