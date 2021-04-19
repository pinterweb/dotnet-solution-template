using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessApp.WebApi.ProblemDetails
{
    public static class ProblemDetailExtensions
    {
        private static string[] KnownKeys = typeof(ProblemDetail).GetProperties().Select(p => p.Name).ToArray();

        public static IDictionary<string, object> GetExtensions(this ProblemDetail problem)
        {
            return problem.Where(kvp => !KnownKeys.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
