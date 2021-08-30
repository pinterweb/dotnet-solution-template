using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Handles starting and committing a transaction for the scope of the request
    /// </summary>
    public class TransactionRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly IPostCommitHandler<TRequest, TResponse> postHandler;
        private readonly IUnitOfWork uow;

        public TransactionRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IUnitOfWork uow, IPostCommitHandler<TRequest, TResponse> postHandler)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.uow = uow.NotNull().Expect(nameof(uow));
            this.postHandler = postHandler.NotNull().Expect(nameof(postHandler));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            var handlerResult = await inner.HandleAsync(request, cancelToken);

            return handlerResult.Kind switch
            {
                ValueKind.Ok => await CommitAsync(request, handlerResult, cancelToken),
                ValueKind.Error => handlerResult
            };
        }

        private async Task<Result<TResponse, Exception>> CommitAsync(TRequest request,
            Result<TResponse, Exception> result, CancellationToken cancelToken)
        {
            // do not revert the first commit, no reason to
            var response = result.Unwrap();
            await uow.CommitAsync(cancelToken);
            bool revertRun = false;

            try
            {
                return await postHandler.HandleAsync(request, response, cancelToken)
                    .OrElseRunAsync(u => revertRun = true)
                    .OrElseRunAsync(u => uow.RevertAsync(cancelToken))
                    .AndThenRunAsync(u => uow.CommitAsync(cancelToken))
                    .AndThenAsync(u => result);
            }
            catch
            {
                if (!revertRun) await uow.RevertAsync(cancelToken);
                throw;
            }
        }
    }
}
