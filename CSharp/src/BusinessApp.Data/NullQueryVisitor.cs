namespace BusinessApp.Data
{
    using System.Linq;

    public sealed class NullQueryVisitor<TResult> : IQueryVisitor<TResult>
        where TResult : class
    {
        public IQueryable<TResult> Visit(IQueryable<TResult> dbSet) => dbSet;
    }
}
