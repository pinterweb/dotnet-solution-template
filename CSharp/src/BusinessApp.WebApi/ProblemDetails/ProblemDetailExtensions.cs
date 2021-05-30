using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessApp.WebApi.ProblemDetails
{
    /// <summary>
    /// Extension methods for <see cref="ProblemDetail" />
    /// </summary>
    public static class ProblemDetailExtensions
    {
        private static readonly string[] knownKeys
            = typeof(ProblemDetail).GetProperties().Select(p => p.Name).ToArray();

        public static IDictionary<string, object> GetExtensions(this ProblemDetail problem)
            => problem
                .Where(kvp => !knownKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
