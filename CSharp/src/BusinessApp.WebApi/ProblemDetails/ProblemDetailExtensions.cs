namespace BusinessApp.WebApi.ProblemDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ProblemDetailExtensions
    {
        private static string[] KnownKeys = typeof(ProblemDetail).GetProperties().Select(p => p.Name).ToArray();

        public static IDictionary<string, object> GetExtensions(this ProblemDetail problem)
        {
            return problem.Where(kvp => !KnownKeys.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
