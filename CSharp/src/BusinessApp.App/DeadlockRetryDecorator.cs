namespace BusinessApp.App
{
    using System;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Retries handling logic if a deadlock raised an exception
    /// </summary>
    public class DeadlockRetryDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> decoratee;
        private readonly int sleepBetweenRetries = 500;
        private int retries = 5;

        public DeadlockRetryDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.decoratee = Guard.Against.Null(decoratee).Expect(nameof(decoratee));
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await HandleWithRetry(command, cancellationToken);
        }

        private async Task HandleWithRetry(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await decoratee.HandleAsync(command, cancellationToken);
            }
            catch (Exception ex)
            {
                if (!IsDeadlockException(ex)) throw;

                retries--;

                if (retries <= 0)
                {
                    throw new CommunicationException(
                        "There was a conflict saving your data. Please retry your " +
                        "operation again. If you continue to see this message, please " +
                        "contact support.", ex);

                }

                Thread.Sleep(sleepBetweenRetries);

                await HandleWithRetry(command, cancellationToken);
            }
        }

        private static bool IsDeadlockException(Exception ex)
        {
            return ex is DbException
                && ex.Message.Contains("deadlock")
                ? true
                : ex.InnerException == null
                    ? false
                    : IsDeadlockException(ex.InnerException);
        }
    }
}
