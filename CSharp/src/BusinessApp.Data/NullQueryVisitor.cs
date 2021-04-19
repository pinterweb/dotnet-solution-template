using System.Linq;

namespace BusinessApp.Data
{
    public sealed class NullQueryVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        public IQueryable<TResult> Visit(IQueryable<TResult> dbSet) => dbSet;
    }
}
