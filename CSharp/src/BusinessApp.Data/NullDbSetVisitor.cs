namespace BusinessApp.Data
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public sealed class NullDbSetVisitorFactory<TQuery, TResult> : IDbSetVisitorFactory<TQuery, TResult>
        where TResult : class
    {
        private static readonly NullDbSetVisitor Visitor = new NullDbSetVisitor();

        public IDbSetVisitor<TResult> Create(TQuery query) => Visitor;

        private class NullDbSetVisitor : IDbSetVisitor<TResult>
        {
            public IQueryable<TResult> Visit(DbSet<TResult> dbSet) => dbSet;
        }
    }
}
