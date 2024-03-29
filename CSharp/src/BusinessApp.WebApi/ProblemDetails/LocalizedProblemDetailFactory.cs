using System;
using System.Collections;
using System.Collections.Generic;
using BusinessApp.Kernel;
using Microsoft.Extensions.Localization;

namespace BusinessApp.WebApi.ProblemDetails
{
    /// <summary>
    /// Localizes a <see cref="ProblemDetail" /> response
    /// </summary>
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

        public ProblemDetail Create(Exception exception)
        {
            var problem = inner.Create(exception);
            var detail = problem.Detail == null ? null : localizer[problem.Detail];

            problem.Detail = detail?.Value;

            foreach (var kvp in problem.GetExtensions())
            {
                var extValue = TranslateExtension(kvp.Value) ?? "";
                problem[kvp.Key] = extValue;
            }

            return problem;
        }

        private object? TranslateExtension(object? value) => value switch
        {
            IDictionary d => TranslateExtension(d),
            IEnumerable e when e is not string => TranslateExtension(e),
            null => null,
            object o when o.ToString() is null => null,
            _ => localizer[value.ToString()!].Value,
        };


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
