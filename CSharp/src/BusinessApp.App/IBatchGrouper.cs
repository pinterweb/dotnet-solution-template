namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Consolidate batch requests into units of work
    /// </summary>
    public interface IBatchGrouper<TCommand>
    {
        /// <summary>
        /// Groups the <paramref name="commands"/>
        /// </summary>
        /// <param name="commands">The commands to group</param>
        /// <returns>The new grouped commands</returns>
        Task<IEnumerable<IEnumerable<TCommand>>> GroupAsync(
            IEnumerable<TCommand> commands,
            CancellationToken cancelToken);
    }
}
