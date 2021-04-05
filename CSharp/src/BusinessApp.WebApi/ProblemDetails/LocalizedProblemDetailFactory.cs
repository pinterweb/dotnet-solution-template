namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using Microsoft.Extensions.Localization;

    public class LocalizedProblemDetailFactory : IProblemDetailFactory
    {
        private readonly IProblemDetailFactory inner;
        private readonly IStringLocalizer localizer;

        public LocalizedProblemDetailFactory(IProblemDetailFactory inner,
            IStringLocalizer localizer)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.localizer = localizer.NotNull().Expect(nameof(localizer));
        }

        public ProblemDetail Create(Exception error)
        {
            var problem = inner.Create(error);
            var detail = problem.Detail == null ? null : localizer[problem.Detail];

            var localizedProblem = new ProblemDetail(problem.StatusCode, problem.Type)
            {
                Detail = detail?.Value
            };

            foreach (var kvp in problem.GetExtensions())
            {
                var extValue = TranslateExtension(kvp.Value) ?? "";
                localizedProblem[kvp.Key] = extValue;
            }

            return localizedProblem;
        }

        private object? TranslateExtension(object? value)
        {
            return value switch
            {
                IDictionary d => TranslateExtension(d),
                IEnumerable e when e is not string => TranslateExtension(e),
                null => null,
                object o when o.ToString() is null => null,
                _ => localizer[value.ToString()!].Value,
            };
        }


        private object TranslateExtension(IDictionary dic)
        {
            var genericDic = new Dictionary<string, object?>();

            foreach (DictionaryEntry kvp in dic)
            {
                genericDic[kvp.Key?.ToString() ?? ""] = TranslateExtension(kvp.Value);
            }

            return genericDic;
        }

        private object TranslateExtension(IEnumerable enumerable)
        {
            var genericList = new List<object?>();

            foreach (var item in enumerable)
            {
                genericList.Add(TranslateExtension(item));
            }

            return genericList;
        }
    }
}
