namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchCommandHandler<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly ICommandHandler<TCommand> inner;

        public BatchCommandHandler(ICommandHandler<TCommand> inner)
        {
            this.inner = GuardAgainst.Null(inner, nameof(inner));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            foreach (var c in command)
            {
                await inner.HandleAsync(c, cancellationToken);
            }
        }
    }
}
