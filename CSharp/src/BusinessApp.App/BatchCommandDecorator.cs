namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Runs multiple command handlers
    /// </summary>
    public class BatchCommandDecorator<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly ICommandHandler<TCommand> decoratedHandler;

        public BatchCommandDecorator(ICommandHandler<TCommand> decoratedHandler)
        {
            this.decoratedHandler = GuardAgainst.Null(decoratedHandler, nameof(decoratedHandler));
        }

        public virtual async Task HandleAsync(IEnumerable<TCommand> commands,
            CancellationToken cancellationToken)
        {
            var handlerTasks = new List<Task>();

            foreach (var c in commands)
            {
                handlerTasks.Add(decoratedHandler.HandleAsync(c, cancellationToken));
            }

            await Task.WhenAll(handlerTasks);
        }
    }
}
