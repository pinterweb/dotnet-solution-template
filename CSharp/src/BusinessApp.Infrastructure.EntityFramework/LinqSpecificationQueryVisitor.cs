using System.Linq;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Filters the query
    /// </summary>
    public class LinqSpecificationQueryVisitor<T> : IQueryVisitor<T>
    {
        private readonly LinqSpecification<T> spec;

        public LinqSpecificationQueryVisitor(LinqSpecification<T> spec)
        {
            this.spec = spec.NotNull().Expect(nameof(spec));
        }

        public IQueryable<T> Visit(IQueryable<T> query) => query.Where(spec.Predicate);
    }
}
