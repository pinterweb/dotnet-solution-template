namespace BusinessApp.Infrastructure.EntityFramework
{
    public interface IDbSetVisitorFactory<in TQuery, TResult>
        where TQuery : notnull
        where TResult : class
    {
        IDbSetVisitor<TResult> Create(TQuery query);
    }
}
