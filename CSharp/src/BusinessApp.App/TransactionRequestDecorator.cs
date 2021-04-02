namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

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
            IRequestHandler<TRequest, TResponse> inner,
            PostCommitRegister register,
            ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.transactionFactory = transactionFactory.NotNull().Expect(nameof(transactionFactory));
            this.register = register.NotNull().Expect(nameof(register));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(
            TRequest command, CancellationToken cancelToken)
        {
            var trans = transactionFactory.Begin();

            var result = await inner.HandleAsync(command, cancelToken);

            await trans.CommitAsync(cancelToken);

            while (register.FinishHandlers.Count > 0)
            {
                try
                {
                    await register.OnFinishedAsync();
                    await trans.CommitAsync(cancelToken);
                }
                catch
                {

                    try
                    {
                        await trans.RevertAsync(cancelToken);
                    }
                    catch (Exception revertError)
                    {
                        logger.Log(new LogEntry(LogSeverity.Critical, revertError.Message)
                        {
                            Exception = revertError,
                            Data = command
                        });
                    }

                    throw;
                }
            }

            return result;
        }
    }
}
