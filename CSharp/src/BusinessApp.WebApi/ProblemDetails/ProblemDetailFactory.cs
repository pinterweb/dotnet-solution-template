using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi.ProblemDetails
{
    /// <summary>
    /// Default implementation for the <see cref="IProblemDetailFactory" /> service
    /// </summary>
    public class ProblemDetailFactory : IProblemDetailFactory
    {
        private readonly HashSet<ProblemDetailOptions> options;

        public ProblemDetailFactory(HashSet<ProblemDetailOptions> options)
            => this.options = options.NotNull().Expect(nameof(options));

        public ProblemDetail Create(Exception exception)
        {
            var error = exception.NotNull().Expect(nameof(exception));

            var option = error switch
            {
                StatusCodeException s => CreateOptionsFromStatusCode(s),
                _ => CreateDefaultOptions(error)
            };

            if (options.TryGetValue(option, out var actualValue))
            {
                option = actualValue;
            }

#if DEBUG
            return exception is BatchException f
                ? CreateCompositeProblem(f)
                : CreateSingleProblem(exception, option);
#elif hasbatch
            return exception is BatchException f
                ? CreateCompositeProblem(f)
                : CreateSingleProblem(exception, option);
#else
            return CreateSingleProblem(exception, option);
#endif
        }

        private static ProblemDetailOptions CreateDefaultOptions(Exception error)
            => new(error.GetType(), StatusCodes.Status500InternalServerError)
            {
                MessageOverride = error.Message ??
                    "An unknown error has occurred. Please try again or " +
                    "contact support"
            };

        private static ProblemDetailOptions CreateOptionsFromStatusCode(StatusCodeException error)
            => new(error.GetType(), error.StatusCode)
            {
                MessageOverride = error.Message
            };

        private static ProblemDetail CreateSingleProblem(Exception? error,
            ProblemDetailOptions option)
        {
            var type = option.AbsoluteType == null ? null : new Uri(option.AbsoluteType);
            var problem = new ProblemDetail(option.StatusCode, type)
            {
                Detail = option.MessageOverride ?? error?.Message,
            };

            if (error != null)
            {
                foreach (DictionaryEntry entry in error.Data)
                {
                    var key = entry.Key.ToString() ?? CreateKey(option);
                    _ = problem.TryAdd(
                        string.IsNullOrWhiteSpace(key) ? CreateKey(option) : key,
                        entry.Value ?? "");
                }
            }

            return problem;
        }

#if DEBUG
        private CompositeProblemDetail CreateCompositeProblem(BatchException errors)
        {
            var responses = errors
                .Select(r =>
                    r.MapOrElse(
                        err => Create(err),
                        ok => new ProblemDetail(StatusCodes.Status200OK)
                    )
                );

            return new(responses)
            {
                Detail = "The request partially succeeded. Please review the errors before " +
                    "continuing"
            };
        }
#elif hasbatch
        private CompositeProblemDetail CreateCompositeProblem(BatchException errors)
        {
            var responses = errors
                .Select(r =>
                    r.MapOrElse(
                        err => Create(err),
                        ok => new ProblemDetail(StatusCodes.Status200OK)
                    )
                );

            return new(responses)
            {
                Detail = "The request partially succeeded. Please review the errors before " +
                    "continuing"
            };
        }
#endif

    private static string CreateKey(ProblemDetailOptions options)
        => options.StatusCode switch
        {
            400 => ModelValidationException.ValidationKey,
            _ => "Errors"
        };
    }
}
