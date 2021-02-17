namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class NullBatchGrouper<TCommand> : IBatchGrouper<TCommand>
    {
        public Task<IEnumerable<IEnumerable<TCommand>>> GroupAsync(IEnumerable<TCommand> commands,
            CancellationToken cancelToken)
        {
            IEnumerable<IEnumerable<TCommand>> onlyGroup = new IEnumerable<TCommand>[1] { commands };

            return Task.FromResult(onlyGroup);
        }
    }
}
