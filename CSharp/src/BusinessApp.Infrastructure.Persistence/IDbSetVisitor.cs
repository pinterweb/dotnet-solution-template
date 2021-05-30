using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Visits a <see cref="DbSet{T}" /> to provide additional functionality
    /// </summary>
    public interface IDbSetVisitor<T>
        where T : class
    {
        IQueryable<T> Visit(DbSet<T> dbSet);
    }
}
