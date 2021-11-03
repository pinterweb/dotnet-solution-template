using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Consolidate batch requests into units of work
    /// </summary>
    public interface IBatchGrouper<TRequest> where TRequest : notnull
    {
        /// <summary>
        /// Groups the <paramref name="requests"/>
        /// </summary>
        /// <param name="requests">The requests to group</param>
        /// <returns>The new grouped requests</returns>
        Task<IEnumerable<IEnumerable<TRequest>>> GroupAsync(IEnumerable<TRequest> requests,
            CancellationToken cancelToken);
    }
}
