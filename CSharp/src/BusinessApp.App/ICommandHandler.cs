namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles command data in the <typeparam name="TCommand">TCommand</typeparam>
    /// </summary>
    public interface ICommandHandler<in TCommand>
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}
