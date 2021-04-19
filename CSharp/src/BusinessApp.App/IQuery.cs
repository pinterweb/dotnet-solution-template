using System.Collections.Generic;

namespace BusinessApp.App
{
    /// <summary>
    /// Defines a query message
    /// </summary>
    /// <typeparam name="TResult">
    /// The result set type returned from the query
    /// </typeparam>
    public interface IQuery
    {
        int? Limit { get; set; }
        int? Offset { get; set; }
        IEnumerable<string> Sort { get; set; }
        IEnumerable<string> Embed { get; set; }
        IEnumerable<string> Expand { get; set; }
        IEnumerable<string> Fields { get; set; }
    }
}
