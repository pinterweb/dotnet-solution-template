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
        async Task<Result<TCommand, IFormattable>> IRequestHandler<TCommand, TCommand>.HandleAsync(
            TCommand request, CancellationToken cancelToken)
        {
            var result = await RunAsync(request, cancelToken);

            return result.Into().MapOrElse(
                err => Result<TCommand, IFormattable>.Error(err),
                ok => Result<TCommand, IFormattable>.Ok(request)
            );
        }

        Task<Result> RunAsync(TCommand request, CancellationToken cancellationToken);
    }
}
