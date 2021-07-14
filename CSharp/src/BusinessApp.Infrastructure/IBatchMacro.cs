using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service to expand a request into a batch request
    /// </summary>
    public interface IBatchMacro<TMacro, TCommand>
        where TMacro : notnull, IMacro<TCommand>
    {
        /// <summary>
        /// Expands the macro into an batch of new requests
        /// </summary>
        /// <returns>The new commands to run</returns>
        Task<IEnumerable<TCommand>> ExpandAsync(TMacro macro,
            CancellationToken cancelToken);
    }
}
