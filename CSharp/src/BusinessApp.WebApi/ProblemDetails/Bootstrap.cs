using System;
using System.Collections.Generic;
using System.Data;
using System.Security;
using System.Threading.Tasks;
using BusinessApp.App;
using BusinessApp.Domain;
using Microsoft.AspNetCore.Http;
using SimpleInjector;

namespace BusinessApp.WebApi.ProblemDetails
{
    public static partial class ProblemDetailOptionBootstrap
    {
        public static HashSet<ProblemDetailOptions> KnownProblems = new HashSet<ProblemDetailOptions>
        {
            new ProblemDetailOptions(typeof(ActivationException), StatusCodes.Status404NotFound)
            {
                MessageOverride =
                    "You tried to access an unknown part of the application. " +
                    "There no is business logic to handle your request. Please ensure " +
                    "you have the correct address."
            },
            new ProblemDetailOptions(typeof(EntityNotFoundException), StatusCodes.Status404NotFound)
            {
                MessageOverride =
                    "The resource you are looking for was not found at this location." +
                    "Ensure it exists and has not moved since your last request."
            },
            new ProblemDetailOptions(typeof(BadStateException), StatusCodes.Status400BadRequest),
            new ProblemDetailOptions(typeof(FormatException), StatusCodes.Status400BadRequest)
            {
                MessageOverride = "One of your data fields was not in the correct format " +
                    "and was rejected. Please check your data before resubmtting"
            },
            new ProblemDetailOptions(typeof(InvalidOperationException), StatusCodes.Status400BadRequest)
            {
                MessageOverride =
                    "Your request was rejected because of bad data. Please review your " +
                    "request before resubmission."
            },
            new ProblemDetailOptions(typeof(TaskCanceledException), StatusCodes.Status400BadRequest)
            {
                MessageOverride =
                    "Your request has been cancelled. This is most likely due to a bad request. " +
                    "If more details are not provided check the state of your date and retry the request."
            },
            new ProblemDetailOptions(typeof(ModelValidationException), StatusCodes.Status400BadRequest),
            new ProblemDetailOptions(typeof(SecurityException), StatusCodes.Status403Forbidden)
            {
                MessageOverride = "You have insufficient privileges to access this part of the application."
            },
            new ProblemDetailOptions(typeof(SecurityResourceException), StatusCodes.Status403Forbidden),
            new ProblemDetailOptions(typeof(DBConcurrencyException), StatusCodes.Status409Conflict)
            {
                MessageOverride = "You attempted to change data that has already been changed since " +
                    "you started work. Please refresh your application to ensure your data " +
                    "is still available."

            },
            new ProblemDetailOptions(typeof(NotImplementedException), StatusCodes.Status501NotImplemented)
            {
                MessageOverride = "Whoops! Your workflow is available, but we have not " +
                    "got around to implementing any of your business logic yet. Check back " +
                    "later."
            },
            new ProblemDetailOptions(typeof(NotSupportedException), StatusCodes.Status501NotImplemented)
            {
                MessageOverride = "The application does not support this business workflow."
            },
            new ProblemDetailOptions(typeof(CommunicationException), StatusCodes.Status424FailedDependency)
        };
    }
}
