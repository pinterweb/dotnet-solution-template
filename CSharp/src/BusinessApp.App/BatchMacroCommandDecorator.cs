namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchMacroCommandDecorator<TMacro, TCommand> : ICommandHandler<TMacro>
        where TMacro : IMacro<TCommand>
    {
        private readonly IBatchMacro<TMacro, TCommand> expander;
        private readonly ICommandHandler<IEnumerable<TCommand>> handler;

        public BatchMacroCommandDecorator(
            IBatchMacro<TMacro, TCommand> expander,
            ICommandHandler<IEnumerable<TCommand>> handler)
        {
            this.expander = Guard.Against.Null(expander).Expect(nameof(expander));
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task HandleAsync(TMacro command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));

            var payloads = await expander.ExpandAsync(command, cancellationToken);

            if (!payloads.Any())
            {
                throw new ValidationException(
                    "The request expected to change records, but no records were found."
                );
            }

            await handler.HandleAsync(payloads, cancellationToken);
        }
    }
}
