namespace BusinessApp.Data
{
    using BusinessApp.App;

    /// <summary>
    /// Factory to create an Entity Framework query visitor
    /// </summary>
    public class EFQueryVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : Query
        where TResult : class
    {
        public IQueryVisitor<TResult> Create(TQuery query) => new EFQueryVisitor<TResult>(query);
    }
}
