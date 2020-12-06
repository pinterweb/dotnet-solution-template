namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public class ProblemDetailFactory : IProblemDetailFactory
    {
        private readonly HashSet<ProblemDetailOptions> options;

        public ProblemDetailFactory(HashSet<ProblemDetailOptions> options)
        {
            this.options = options.NotNull().Expect(nameof(options));
        }

        public ProblemDetail Create(IFormattable error)
        {
            var option = new ProblemDetailOptions()
            {
                MessageOverride =
                    error?.ToString("G", null) ??
                    "An unknown error has occurred. Please try again or " +
                    "contact support",
                StatusCode = StatusCodes.Status500InternalServerError,
                ProblemType = error?.GetType()
            };

            if (options.TryGetValue(option, out ProblemDetailOptions actualValue))
            {
                option = actualValue;
            }

            if (error is IEnumerable<Result> f)
            {
                return CreateCompositeProblem(f);
            }

            return CreateSingleProblem(error, option);
        }

        private static ProblemDetail CreateSingleProblem(IFormattable error, ProblemDetailOptions option)
        {
            var type = option.AbsoluteType == null ? null : new Uri(option.AbsoluteType);
            var problem = new ProblemDetail(option.StatusCode, type)
            {
                Detail = option.MessageOverride ?? error?.ToString("g", null),
            };

            if (error is Exception e)
            {
                foreach (DictionaryEntry entry in e.Data)
                {
                    string keyVal = entry.Key switch
                    {
                        IFormattable f => f.ToString("g", null),
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

        private ProblemDetail CreateCompositeProblem(IEnumerable<Result> errors)
        {
            if (errors.All(e => e.Kind == ValueKind.Ok))
            {
                throw new BusinessAppWebApiException("A multi status problem should have at least one error");
            }

            var responses = errors
                .Select(r =>
                    r.Into().MapOrElse(
                        err => Create(err),
                        ok => new ProblemDetail(StatusCodes.Status200OK)
                    )
                );

            return new CompositeProblemDetail(responses)
            {
                Detail = errors is IFormattable ? errors.ToString() : null
            };
        }
    }
}
