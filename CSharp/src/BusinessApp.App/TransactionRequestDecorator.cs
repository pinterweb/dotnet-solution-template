namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class TransactionRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
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

        public async Task<Result<TResponse, IFormattable>> HandleAsync(
            TRequest command, CancellationToken cancellationToken)
        {
            command.NotNull().Expect(nameof(command));

            var trans = transactionFactory.Begin();

            var result = await inner.HandleAsync(command, cancellationToken);

            await trans.CommitAsync(cancellationToken);

            while (register.FinishHandlers.Count > 0)
            {
                try
                {
                    await register.OnFinishedAsync();
                    await trans.CommitAsync(cancellationToken);
                }
                catch
                {

                    try
                    {
                        await trans.RevertAsync(cancellationToken);
                    }
                    catch (Exception revertError)
                    {
                        logger.Log(
                            new LogEntry(
                                LogSeverity.Critical,
                                revertError.Message,
                                revertError,
                                command
                            )
                        );
                    }

                    throw;
                }
            }

            return result;
        }
    }
}
