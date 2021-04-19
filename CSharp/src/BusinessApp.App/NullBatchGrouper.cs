using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.App
{
    public class NullBatchGrouper<TCommand> : IBatchGrouper<TCommand>
        where TCommand : notnull
    {
        public Task<IEnumerable<IEnumerable<TCommand>>> GroupAsync(IEnumerable<TCommand> commands,
            CancellationToken cancelToken)
        {
            IEnumerable<IEnumerable<TCommand>> onlyGroup = new IEnumerable<TCommand>[1] { commands };

            return Task.FromResult(onlyGroup);
        }
    }
}
