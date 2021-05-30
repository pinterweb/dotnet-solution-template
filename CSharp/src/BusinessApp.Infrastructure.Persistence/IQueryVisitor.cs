using System.Linq;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Visits an IQueryable, returning a new queryable
    /// </summary>
    public interface IQueryVisitor<T>
    {
        IQueryable<T> Visit(IQueryable<T> queryable);
    }
}
