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
        async Task<Result<TCommand, Exception>> IRequestHandler<TCommand, TCommand>.HandleAsync(
            TCommand request, CancellationToken cancelToken)
        {
            var result = await RunAsync(request, cancelToken);

            return result.MapOrElse(
                err => Result.Error<TCommand>(err),
                ok => Result.Ok(request)
            );
        }

        Task<Result<Unit, Exception>> RunAsync(TCommand request,
            CancellationToken cancellationToken);
    }
}
