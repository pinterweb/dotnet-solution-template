using System.Collections.Generic;
using System.Linq;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Combines all specifications with the and operator
    /// </summary>
    public class AndLinqSpecificationBuilder<TQuery, TResult> : ILinqSpecificationBuilder<TQuery, TResult>
        where TQuery : notnull
    {
        private readonly IEnumerable<ILinqSpecificationBuilder<TQuery, TResult>> builders;

        public AndLinqSpecificationBuilder(IEnumerable<ILinqSpecificationBuilder<TQuery, TResult>> builders)
            => this.builders = builders.NotNull().Expect(nameof(builders));

        public LinqSpecification<TResult> Build(TQuery query)
        {
            var allSpecs = new List<LinqSpecification<TResult>>();

            foreach (var builder in builders)
            {
                allSpecs.Add(builder.Build(query));
            }

            return allSpecs.Any()
                ? allSpecs.Aggregate((current, next) => current & next)
                : new NullSpecification<TResult>(true);
        }
    }
}
