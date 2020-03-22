namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Validates each command in the batch
    /// </summary>
    public class ValidationBatchCommandDecorator<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly IValidator<TCommand> validator;
        private readonly ICommandHandler<IEnumerable<TCommand>> handler;

        public ValidationBatchCommandDecorator(IValidator<TCommand> validator,
            ICommandHandler<IEnumerable<TCommand>> handler)
        {
            this.validator = GuardAgainst.Null(validator, nameof(validator));
            this.handler = GuardAgainst.Null(handler, nameof(handler));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            foreach(var cmd in command) validator.ValidateObject(cmd);

            await handler.HandleAsync(command, cancellationToken);
        }
    }
}
