namespace BusinessApp.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Builds one factory from many query visitor factories
    /// </summary>
    public sealed class CompositeQueryVisitorBuilder<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
    {
        private readonly IEnumerable<IQueryVisitorFactory<TQuery, TResult>> factories;

        public CompositeQueryVisitorBuilder(IEnumerable<IQueryVisitorFactory<TQuery, TResult>> factories)
        {
            this.factories = GuardAgainst.Null(factories, nameof(factories));
        }

        public IQueryVisitor<TResult> Create(TQuery query)
        {
            var visitors = factories.Select(f => f.Create(query));

            return new CompositeQueryVisitor<TResult>(visitors);
        }
    }
}
