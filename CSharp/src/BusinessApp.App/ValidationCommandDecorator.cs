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

        public Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            validator.ValidateObject(command);

            return handler.HandleAsync(command, cancellationToken);
        }
    }
}
