namespace BusinessApp.Data
{
    /// <summary>
    /// Create  a query visitor from the query at runtime
    /// </summary>
    public interface IQueryVisitorFactory<TQuery, TResult>
    {
        IQueryVisitor<TResult> Create(TQuery query);
    }
}
