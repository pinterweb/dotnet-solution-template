using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.App
{
    /// <summary>
    /// Consolidate batch requests into units of work
    /// </summary>
    public interface IBatchMacro<TMacro, TCommand>
        where TMacro : notnull, IMacro<TCommand>
    {
        /// <summary>
        /// Groups the <paramref name="commands"/>
        /// </summary>
        /// <param name="commands">The commands to group</param>
        /// <returns>The new grouped commands</returns>
        Task<IEnumerable<TCommand>> ExpandAsync(TMacro macro,
            CancellationToken cancelToken);
    }
}
