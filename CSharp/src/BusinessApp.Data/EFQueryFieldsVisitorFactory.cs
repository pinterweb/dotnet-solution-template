namespace BusinessApp.Data
{
    using BusinessApp.App;

    /// <summary>
    /// Factory to create the fields query visitor
    /// </summary>
    public class EFQueryFieldsVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : Query
        where TResult : class
    {
        public IQueryVisitor<TResult> Create(TQuery query) => new EFQueryFieldsVisitor<TResult>(query);
    }
}
