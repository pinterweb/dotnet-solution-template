using BusinessApp.Domain;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Implementation to support visitors that do not need to be constructed
    /// from the runtime <typeparam name="TQuery"/>
    /// </summary>
    public sealed class ConstructedQueryVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : notnull
        where TResult : class
    {
        private readonly IQueryVisitor<TResult> visitor;

        public ConstructedQueryVisitorFactory(IQueryVisitor<TResult> visitor)
        {
            this.visitor = visitor.NotNull().Expect(nameof(visitor));
        }

        public IQueryVisitor<TResult> Create(TQuery query) => visitor;
    }
}
