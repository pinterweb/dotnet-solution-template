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
            this.validator = validator.NotNull().Expect(nameof(validator));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResult, IFormattable>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            request.NotNull().Expect(nameof(request));

            var result = await validator.ValidateAsync(request, cancelToken);

            return result.Kind switch
            {
                ValueKind.Error => result.Into<TResult>(),
                ValueKind.Ok => await inner.HandleAsync(request, cancelToken),
                _ => throw new NotImplementedException()
            };
        }
    }
}
