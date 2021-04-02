namespace BusinessApp.Data
{
    /// <summary>
    /// Create  a query visitor from the query at runtime
    /// </summary>
    public interface IQueryVisitorFactory<in TQuery, TResult>
        where TQuery : notnull
    {
        IQueryVisitor<TResult> Create(TQuery query);
    }
}
