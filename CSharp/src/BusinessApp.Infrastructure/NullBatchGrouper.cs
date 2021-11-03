using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Null pattern that does not group the requests
    /// </summary>
    public class NullBatchGrouper<TRequest> : IBatchGrouper<TRequest>
        where TRequest : notnull
    {
        public Task<IEnumerable<IEnumerable<TRequest>>> GroupAsync(IEnumerable<TRequest> requests,
            CancellationToken cancelToken)
        {
            IEnumerable<IEnumerable<TRequest>> onlyGroup = new IEnumerable<TRequest>[1] { requests };

            return Task.FromResult(onlyGroup);
        }
    }
}
