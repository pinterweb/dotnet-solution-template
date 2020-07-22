namespace BusinessApp.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;

    /// <summary>
    /// Builds one visitor to run many query visitors
    /// </summary>
    public class CompositeQueryVisitor<T> : IQueryVisitor<T>
    {
        private readonly IEnumerable<IQueryVisitor<T>> visitors;

        public CompositeQueryVisitor(IEnumerable<IQueryVisitor<T>> visitors)
        {
            this.visitors = GuardAgainst.Null(visitors, nameof(visitors));
        }

        public IQueryable<T> Visit(IQueryable<T> queryable)
        {
            foreach (var visitor in visitors)
            {
                queryable = visitor.Visit(queryable);
            }

            return queryable;
        }
    }
}
