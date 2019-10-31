namespace BusinessApp.Data
{
    using System.Linq;

    /// <summary>
    /// Visits an IQueryable, returning a new queryable
    /// </summary>
    public interface IQueryVisitor<T>
    {
        IQueryable<T> Visit(IQueryable<T> query);
    }
}
