namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Validates the command prior to handling
    /// </summary>
    public class ValidationRequestDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    {
        private readonly IValidator<TRequest> validator;
        private readonly IRequestHandler<TRequest, TResult> inner;

        public ValidationRequestDecorator(IValidator<TRequest> validator, IRequestHandler<TRequest, TResult> inner)
        {
            this.validator = Guard.Against.Null(validator).Expect(nameof(validator));
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
        }

        public async Task<Result<TResult, IFormattable>> HandleAsync(TRequest query,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(query).Expect(nameof(query));

            await validator.ValidateAsync(query, cancellationToken);

            return await inner.HandleAsync(query, cancellationToken);
        }
    }
}
