using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Validates the command prior to handling
    /// </summary>
    public class ValidationRequestDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IValidator<TRequest> validator;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public ValidationRequestDecorator(IValidator<TRequest> validator, IRequestHandler<TRequest, TResponse> inner)
        {
            this.validator = validator.NotNull().Expect(nameof(validator));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            _ = request.NotNull().Expect(nameof(request));

            var result = await validator.ValidateAsync(request, cancelToken);

            return result.Kind switch
            {
                ValueKind.Error => Result.Error<TResponse>(result.UnwrapError()),
                ValueKind.Ok => await inner.HandleAsync(request, cancelToken),
                _ => throw new NotImplementedException()
            };
        }
    }
}
