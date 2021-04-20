using System.Collections.Generic;
using System.Linq;
using BusinessApp.Domain;
using BusinessApp.Infrastructure;

namespace BusinessApp.Data
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
        {
            this.builders = builders.NotNull().Expect(nameof(builders));
        }

        public LinqSpecification<TResult> Build(TQuery query)
        {
            var allSpecs = new List<LinqSpecification<TResult>>();

            foreach (var builder in builders)
            {
                allSpecs.Add(builder.Build(query));
            }

            if (allSpecs.Any())
            {
                return allSpecs.Aggregate((current, next) => current & next);
            }

            return new NullSpecification<TResult>(true);
        }

        public IQueryVisitor<TResult> Create(TQuery query)
        {
            return new LinqSpecificationQueryVisitor<TResult>(
                Build(query)
            );
        }
    }
}
