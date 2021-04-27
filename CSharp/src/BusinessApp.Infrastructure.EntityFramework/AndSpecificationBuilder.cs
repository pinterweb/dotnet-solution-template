using System.Collections.Generic;
using System.Linq;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Combines all specifications with the & operator
    /// </summary>
    public class AndSpecificationBuilder<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>,
        ILinqSpecificationBuilder<TQuery, TResult>
        where TQuery : notnull
    {
        private readonly IEnumerable<ILinqSpecificationBuilder<TQuery, TResult>> builders;

        public AndSpecificationBuilder(IEnumerable<ILinqSpecificationBuilder<TQuery, TResult>> builders)
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

        public IQueryVisitor<TResult> Create(TQuery query)
            => new LinqSpecificationQueryVisitor<TResult>(Build(query));
    }
}
