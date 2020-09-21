namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Validates the command prior to handling
    /// </summary>
    public class ValidationQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IValidator<TQuery> validator;
        private readonly IQueryHandler<TQuery, TResult> inner;

        public ValidationQueryDecorator(IValidator<TQuery> validator, IQueryHandler<TQuery, TResult> inner)
        {
            this.validator = GuardAgainst.Null(validator, nameof(validator));
            this.inner = GuardAgainst.Null(inner, nameof(inner));
        }

        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            GuardAgainst.Null(query, nameof(query));

            await validator.ValidateAsync(query, cancellationToken);

            return await inner.HandleAsync(query, cancellationToken);
        }
    }
}
