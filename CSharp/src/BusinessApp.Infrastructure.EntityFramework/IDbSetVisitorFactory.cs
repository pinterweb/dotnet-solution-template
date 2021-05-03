namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Create a <see cref="IDbSetVisitor" /> at runtime based on the query
    /// </summary>
    public interface IDbSetVisitorFactory<in TQuery, TResult>
        where TQuery : notnull
        where TResult : class
    {
        IDbSetVisitor<TResult> Create(TQuery query);
    }
}
