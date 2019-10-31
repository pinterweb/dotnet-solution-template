namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Responsible for committing and running the post handlers after commit
    /// </summary>
    public class TransactionDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly IUnitOfWork uow;
        private readonly PostHandleRegister register;
        private readonly ICommandHandler<TCommand> handler;

        public TransactionDecorator(IUnitOfWork uow,
            ICommandHandler<TCommand> handler,
            PostHandleRegister register)
        {
            this.uow = GuardAgainst.Null(uow, nameof(uow));
            this.handler = GuardAgainst.Null(handler, nameof(handler));
            this.register = GuardAgainst.Null(register, nameof(register));
        }

        async Task ICommandHandler<TCommand>.HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            await handler.HandleAsync(command, cancellationToken);
            await uow.CommitAsync(cancellationToken);

            try
            {
                // Other handlers could add more to the list so keep looping
                // each time creating a new commit scope
                while (register.FinishHandlers.Count > 0)
                {
                    await register.OnFinishedAsync();
                    await uow.CommitAsync(cancellationToken);
                }
            }
            catch (Exception)
            {
                await uow.RevertAsync(cancellationToken);
                throw;
            }
        }
    }
}
