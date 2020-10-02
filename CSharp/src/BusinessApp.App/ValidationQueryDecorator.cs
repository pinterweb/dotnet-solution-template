namespace BusinessApp.App
{
    using System;
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
            this.validator = Guard.Against.Null(validator).Expect(nameof(validator));
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
        }

        public async Task<Result<TResult, IFormattable>> HandleAsync(TQuery query,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(query).Expect(nameof(query));

            await validator.ValidateAsync(query, cancellationToken);

            return await inner.HandleAsync(query, cancellationToken);
        }
    }
}
