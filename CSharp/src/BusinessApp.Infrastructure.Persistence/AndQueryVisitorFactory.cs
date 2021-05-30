using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Combines all specifications with the and operator
    /// </summary>
    public class AndQueryVisitorFactory<TQuery, TResult> : IQueryVisitorFactory<TQuery, TResult>
        where TQuery : notnull
    {
        private readonly AndLinqSpecificationBuilder<TQuery, TResult> andLinqBuilder;

        public AndQueryVisitorFactory(AndLinqSpecificationBuilder<TQuery, TResult> andLinqBuilder)
            => this.andLinqBuilder = andLinqBuilder.NotNull().Expect(nameof(andLinqBuilder));

        public IQueryVisitor<TResult> Create(TQuery query)
            => new LinqSpecificationQueryVisitor<TResult>(andLinqBuilder.Build(query));
    }
}
