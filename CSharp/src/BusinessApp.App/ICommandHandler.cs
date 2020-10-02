namespace BusinessApp.App
{
    /// <summary>
    /// Handles command data in the <typeparam name="TCommand">TCommand</typeparam>
    /// </summary>
    /// <implementers>
    /// Logic in this pipeline can modify data
    /// </implementers>
    public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, TCommand>
    {
    }
}
