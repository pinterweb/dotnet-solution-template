namespace BusinessApp.Infrastructure.EntityFramework
{
    public sealed class NullDbSetVisitorFactory<TQuery, TResult> : IDbSetVisitorFactory<TQuery, TResult>
        where TQuery : notnull
        where TResult : class
    {
        private static readonly NullDbSetVisitor<TResult> Visitor = new NullDbSetVisitor<TResult>();

        public IDbSetVisitor<TResult> Create(TQuery query) => Visitor;
    }
}
