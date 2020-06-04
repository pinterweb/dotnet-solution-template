namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class TransactionDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> inner;
        private readonly ITransactionFactory transactionFactory;
        private readonly PostCommitRegister register;

        public TransactionDecorator(ITransactionFactory transactionFactory,
            ICommandHandler<TCommand> inner,
            PostCommitRegister register)
        {
            this.inner = GuardAgainst.Null(inner, nameof(inner));
            this.transactionFactory = GuardAgainst.Null(transactionFactory, nameof(transactionFactory));
            this.register = GuardAgainst.Null(register, nameof(register));
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            var trans = transactionFactory.Begin();

            await inner.HandleAsync(command, cancellationToken);

            await trans.CommitAsync(cancellationToken);

            while (register.FinishHandlers.Count > 0)
            {
                try
                {
                    await register.OnFinishedAsync();
                }
                catch
                {
                    await trans.RevertAsync(cancellationToken);
                    throw;
                }

                try
                {
                    await trans.CommitAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    throw new PotentialDataLossException("If this business transaction generated " +
                        "external messages to other system(s), then the two may be out of sync. The " +
                        "app [currently] does not revert exernal systems once a message was sent. " +
                        "If you expected messages to be sent to external systems please verify " +
                        "the data in those systems before proceeding", e);
                }
            }
        }
    }
}
