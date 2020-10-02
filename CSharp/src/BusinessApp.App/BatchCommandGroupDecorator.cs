namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchCommandGroupDecorator<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private static readonly Regex regex = new Regex(@"^\[(\d+)\](\..*)$");
        private readonly IBatchGrouper<TCommand> grouper;
        private readonly ICommandHandler<IEnumerable<TCommand>> handler;

        public BatchCommandGroupDecorator(
            IBatchGrouper<TCommand> grouper,
            ICommandHandler<IEnumerable<TCommand>> handler)
        {
            this.grouper = Guard.Against.Null(grouper).Expect(nameof(grouper));
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task<Result<IEnumerable<TCommand>, IFormattable>> HandleAsync(
            IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));

            var payloads = await grouper.GroupAsync(command, cancellationToken);

            var tasks = new List<(IEnumerable<TCommand>, Task<Result<IEnumerable<TCommand>, IFormattable>>)>();

            foreach (var group in payloads)
            {
                tasks.Add((group, handler.HandleAsync(group, cancellationToken)));
            }

            var _ = await Task.WhenAll(tasks.Select(s => s.Item2));

            var orderedResults = new List<Result<IEnumerable<TCommand>, IFormattable>>();

            foreach (var item in command)
            {
                var target = tasks.Single(t => t.Item1.Contains(item));

                orderedResults.Add(target.Item2.Result);
            }

            if (orderedResults.Any(r => r.Kind == Result.Error))
            {
                return Result<IEnumerable<TCommand>, IFormattable>
                    .Error(new BatchException(
                        orderedResults.Select(o => o.IgnoreValue()
                    )));
            }

            return Result<IEnumerable<TCommand>, IFormattable>
                .Ok(orderedResults.SelectMany(o => o.Unwrap()));
        }
    }
}
