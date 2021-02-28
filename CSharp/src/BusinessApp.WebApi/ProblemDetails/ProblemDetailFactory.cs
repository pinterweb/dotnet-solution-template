namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public class ProblemDetailFactory : IProblemDetailFactory
    {
        private readonly HashSet<ProblemDetailOptions> options;

        public ProblemDetailFactory(HashSet<ProblemDetailOptions> options)
        {
            this.options = options.NotNull().Expect(nameof(options));
        }

        public ProblemDetail Create(Exception error = null)
        {
            var option = new ProblemDetailOptions()
            {
                MessageOverride = error?.Message ??
                    "An unknown error has occurred. Please try again or " +
                    "contact support",
                StatusCode = StatusCodes.Status500InternalServerError,
                ProblemType = error?.GetType()
            };

            if (options.TryGetValue(option, out ProblemDetailOptions actualValue))
            {
                option = actualValue;
            }

            if (error is BatchException f)
            {
                return CreateCompositeProblem(f);
            }

            return CreateSingleProblem(error, option);
        }

        private static ProblemDetail CreateSingleProblem(Exception error,
            ProblemDetailOptions option)
        {
            var type = option.AbsoluteType == null ? null : new Uri(option.AbsoluteType);
            var problem = new ProblemDetail(option.StatusCode, type)
            {
                Detail = option.MessageOverride ?? error?.Message,
            };

            if (error != null)
            {
                foreach (DictionaryEntry entry in error?.Data)
                {
                    string keyVal = entry.Key switch
                    {
                        string k => k,
                        _ => null
                    };

                    if (!string.IsNullOrWhiteSpace(keyVal))
                    {
                        problem.Add(keyVal, entry.Value);
                    }
                }
            }

            return problem;
        }

        private ProblemDetail CreateCompositeProblem(BatchException errors)
        {
            var responses = errors
                .Select(r =>
                    r.MapOrElse(
                        err => Create(err),
                        ok => new ProblemDetail(StatusCodes.Status200OK)
                    )
                );

            return new CompositeProblemDetail(responses)
            {
                Detail = "The request partially succeeded. Please review the errors before " +
                    "continuing"
            };
        }
    }
}
