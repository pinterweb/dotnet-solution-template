using System.Collections.Generic;
using System.Linq;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Builds one factory from many query visitor factories
    /// </summary>
    public sealed class CompositeQueryVisitorBuilder<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : notnull
    {
        private readonly IEnumerable<IQueryVisitorFactory<TQuery, TResult>> factories;

        public CompositeQueryVisitorBuilder(IEnumerable<IQueryVisitorFactory<TQuery, TResult>> factories)
        {
            this.factories = factories.NotNull().Expect(nameof(factories));
        }

        public IQueryVisitor<TResult> Create(TQuery query)
        {
            var visitors = factories.Select(f => f.Create(query));

            return new CompositeQueryVisitor<TResult>(visitors);
        }
    }
}
