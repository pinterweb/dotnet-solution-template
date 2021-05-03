using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Null pattern to visit a <see cref="DbSet{TResult}" />, which does nothing
    /// </summary>
    public sealed class NullDbSetVisitor<TResult> : IDbSetVisitor<TResult>
        where TResult : class
    {
        public IQueryable<TResult> Visit(DbSet<TResult> dbSet) => dbSet;
    }
}
