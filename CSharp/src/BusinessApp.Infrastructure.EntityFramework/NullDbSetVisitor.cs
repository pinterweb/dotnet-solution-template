using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.EntityFramework
{
    public sealed class NullDbSetVisitor<TResult> : IDbSetVisitor<TResult>
        where TResult : class
    {
        public IQueryable<TResult> Visit(DbSet<TResult> dbSet) => dbSet;
    }
}