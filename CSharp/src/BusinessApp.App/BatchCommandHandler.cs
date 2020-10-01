namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchCommandHandler<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly ICommandHandler<TCommand> inner;

        public BatchCommandHandler(ICommandHandler<TCommand> inner)
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
        }

        public async Task<Result<IEnumerable<TCommand>, IFormattable>> HandleAsync(
            IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));

            var results = new List<Result<TCommand, IFormattable>>();

            foreach(var msg in command)
            {
                results.Add(await inner.HandleAsync(msg, cancellationToken));
            }

            if (results.Any(r => r.Kind == Result.Error))
            {
                return Result<IEnumerable<TCommand>, IFormattable>
                    .Error(new BatchException(
                        results.Select(o => o.IgnoreValue()
                    )));
            }

            return Result<IEnumerable<TCommand>, IFormattable>
                .Ok(results.Select(o => o.Unwrap()));
        }
    }
}
