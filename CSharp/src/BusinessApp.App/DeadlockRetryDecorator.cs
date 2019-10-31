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
        private readonly int sleepBetweenRetries = 300;
        private int retries = 5;

        public DeadlockRetryDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.decoratee = GuardAgainst.Null(decoratee, nameof(decoratee));
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
                if (retries <= 0 || !IsDeadlockException(ex)) throw;

                Thread.Sleep(sleepBetweenRetries);

                retries--;
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
