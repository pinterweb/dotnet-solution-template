namespace BusinessApp.Data
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public interface IDbSetVisitor<T>
        where T : class
    {
        IQueryable<T> Visit(DbSet<T> dbSet);
    }
}
