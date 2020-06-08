namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Validates the command prior to handling
    /// </summary>
    public class ValidationCommandDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly IValidator<TCommand> validator;
        private readonly ICommandHandler<TCommand> handler;

        public ValidationCommandDecorator(IValidator<TCommand> validator, ICommandHandler<TCommand> handler)
        {
            this.validator = GuardAgainst.Null(validator, nameof(validator));
            this.handler = GuardAgainst.Null(handler, nameof(handler));
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            await validator.ValidateAsync(command, cancellationToken);

            await handler.HandleAsync(command, cancellationToken);
        }
    }
}
