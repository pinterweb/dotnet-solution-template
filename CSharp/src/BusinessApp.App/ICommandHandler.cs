namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Handles command data in the <typeparam name="TCommand">TCommand</typeparam>
    /// </summary>
    /// <implementers>
    /// Logic in this pipeline can modify data
    /// </implementers>
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, TCommand>
    {
        async Task<Result<TCommand, IFormattable>> IRequestHandler<TCommand, TCommand>.HandleAsync(TCommand request, CancellationToken cancellationToken)
            {
                await RunAsync(request, cancellationToken);
                return Result<TCommand, IFormattable>.Ok(request);
            }

        Task RunAsync(TCommand request, CancellationToken cancellationToken);
    }
}
