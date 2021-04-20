using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Infrastructure.EntityFramework
{
    public interface IDbSetVisitor<T>
        where T : class
    {
        IQueryable<T> Visit(DbSet<T> dbSet);
    }
}
