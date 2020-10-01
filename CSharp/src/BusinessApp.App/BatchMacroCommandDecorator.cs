namespace BusinessApp.App
{
    using System;
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

        public async Task<Result<TMacro, IFormattable>> HandleAsync(
            TMacro macro,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(macro).Expect(nameof(macro));

            var payloads = await expander.ExpandAsync(macro, cancellationToken);

            if (!payloads.Any())
            {
                throw new BusinessAppAppException(
                    "The macro you ran expected to find records to change, but none were " +
                    "found"
                );
            }

            return (await handler.HandleAsync(payloads, cancellationToken))
                .MapOrElse(
                    err => Result<TMacro, IFormattable>.Error(err),
                    ok => Result<TMacro, IFormattable>.Ok(macro));
        }
    }
}
