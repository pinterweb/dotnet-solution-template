using System.Linq;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Null pattern to visit a queryable and does nothing
    /// </summary>
    public sealed class NullQueryVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        public IQueryable<TResult> Visit(IQueryable<TResult> queryable) => queryable;
    }
}
