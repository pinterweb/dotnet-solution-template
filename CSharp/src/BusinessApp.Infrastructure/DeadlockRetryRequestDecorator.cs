using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Retries handling logic if a deadlock raised an exception
    /// </summary>
    public class DeadlockRetryRequestDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResponse> decoratee;
        private readonly int sleepBetweenRetries = 500;
        private int retries = 5;

        public DeadlockRetryRequestDecorator(IRequestHandler<TRequest, TResponse> decoratee)
        {
            this.decoratee = decoratee.NotNull().Expect(nameof(decoratee));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest command,
            CancellationToken cancelToken)
        {
            return await HandleWithRetry(command, cancelToken);
        }

        private async Task<Result<TResponse, Exception>> HandleWithRetry(
            TRequest command,
            CancellationToken cancellationToken)
        {
            try
            {
                return await decoratee.HandleAsync(command, cancellationToken);
            }
            catch (Exception ex) when (IsDeadlockException(ex))
            {
                retries--;

                if (retries <= 0)
                {
                    return Result.Error<TResponse>(
                        new CommunicationException(
                        "There was a conflict saving your data. Please retry your " +
                        "operation again. If you continue to see this message, please " +
                        "contact support.", ex)
                    );

                }

                Thread.Sleep(sleepBetweenRetries);

                return await HandleWithRetry(command, cancellationToken);
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
