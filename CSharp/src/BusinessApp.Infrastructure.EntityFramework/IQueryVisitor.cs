using System.Linq;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Visits an IQueryable, returning a new queryable
    /// </summary>
    public interface IQueryVisitor<T>
    {
        IQueryable<T> Visit(IQueryable<T> queryable);
    }
}
