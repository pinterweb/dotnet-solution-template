using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Handles starting and committing a transaction for the scope of the request
    /// </summary>
    public class TransactionRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly ITransactionFactory transactionFactory;
        private readonly ILogger logger;
        private readonly PostCommitRegister register;

        public TransactionRequestDecorator(ITransactionFactory transactionFactory,
            IRequestHandler<TRequest, TResponse> inner, PostCommitRegister register,
            ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.transactionFactory = transactionFactory.NotNull().Expect(nameof(transactionFactory));
            this.register = register.NotNull().Expect(nameof(register));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            var uow = transactionFactory.Begin();

            return await inner.HandleAsync(request, cancelToken)
                .AndThenAsync(r => uow.AndThenAsync(u => SaveAsync(u, r, cancelToken)))
                .AndThenAsync(r => RunPostCommitHandlersAsync(uow, request, r, cancelToken));
        }

        private static async Task<Result<TResponse, Exception>> SaveAsync(
            IUnitOfWork uow, TResponse response, CancellationToken cancelToken)
        {
            await uow.CommitAsync(cancelToken);

            return Result.Ok(response);
        }

        private async Task<Result<TResponse, Exception>> RunPostCommitHandlersAsync(
            Result<IUnitOfWork, Exception> uowResult, TRequest request, TResponse response, CancellationToken cancelToken)
        {
            while (register.FinishHandlers.Count > 0)
            {
                try
                {
                    await register.OnFinishedAsync();
                    await uowResult.AndThenAsync(u => SaveAsync(u, response, cancelToken));
                }
                catch
                {

                    try
                    {
                        await RevertAsync(uowResult, response, cancelToken);
                    }
                    catch (Exception revertError)
                    {
                        logger.Log(new LogEntry(LogSeverity.Critical, revertError.Message)
                        {
                            Exception = revertError,
                            Data = request
                        });
                    }

                    throw;
                }
            }

            return Result.Ok(response);
        }

        private static async Task<Result<TResponse, Exception>> RevertAsync(
            Result<IUnitOfWork, Exception> uowResult, TResponse command, CancellationToken cancelToken)
        {
            if (uowResult.Kind == ValueKind.Ok)
            {
                await uowResult.Unwrap().RevertAsync(cancelToken);
            }

            return Result.Ok(command);
        }
    }
}
