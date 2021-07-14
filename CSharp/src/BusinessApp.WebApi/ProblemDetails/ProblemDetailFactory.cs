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
            _ = exception.NotNull().Expect(nameof(exception));

            var option = new ProblemDetailOptions(exception.GetType(),
                StatusCodes.Status500InternalServerError)
            {
                MessageOverride = exception?.Message ??
                    "An unknown error has occurred. Please try again or " +
                    "contact support"
            };

            if (options.TryGetValue(option, out var actualValue))
            {
                option = actualValue;
            }

            return CreateSingleProblem(exception, option);
        }

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
                    var key = entry.Key.ToString() ?? "";
                    _ = problem.TryAdd(key, entry.Value ?? "");
                }
            }

            return problem;
        }
    }
}
