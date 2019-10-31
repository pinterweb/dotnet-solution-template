namespace BusinessApp.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Builds one factory from many query visitor factories
    /// </summary>
    public class CompositeQueryVisitorBuilder<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
    {
        private readonly IEnumerable<IQueryVisitorFactory<TQuery, TResult>> factories;

        public CompositeQueryVisitorBuilder(IEnumerable<IQueryVisitorFactory<TQuery, TResult>> factories)
        {
            this.factories = GuardAgainst.Null(factories, nameof(factories));
        }

        public IQueryVisitor<TResult> Create(TQuery query)
        {
            var visitors = factories.Select(f => f.Create(query));

            return new CompositeQueryVisitor<TQuery, TResult>(visitors);
        }
    }

    /// <summary>
    /// Builds one visitor to run many query visitors
    /// </summary>
    public class CompositeQueryVisitor<TQuery, TResult> : IQueryVisitor<TResult>
    {
        private readonly IEnumerable<IQueryVisitor<TResult>> visitors;

        public CompositeQueryVisitor(IEnumerable<IQueryVisitor<TResult>> visitors)
        {
            this.visitors = GuardAgainst.Null(visitors, nameof(visitors));
        }

        public IQueryable<TResult> Visit(IQueryable<TResult> query)
        {
            foreach (var visitor in visitors)
            {
                query = visitor.Visit(query);
            }

            return query;
        }
    }
}
