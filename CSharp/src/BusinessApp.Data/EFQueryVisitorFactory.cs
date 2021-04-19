using BusinessApp.App;

namespace BusinessApp.Data
{
    /// <summary>
    /// Factory to create an Entity Framework query visitor
    /// </summary>
    public class EFQueryVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : notnull, IQuery
        where TResult : class
    {
        public IQueryVisitor<TResult> Create(TQuery query) =>
            new CompositeQueryVisitor<TResult>(new IQueryVisitor<TResult>[]
            {
                // order matters, .Where().OrderBy().Select()
                new EFQueryVisitor<TResult>(query),
                new EFQuerySortVisitor<TResult>(query),
                new EFQueryFieldsVisitor<TResult>(query)
            });
    }
}
