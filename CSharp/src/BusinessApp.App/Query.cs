using System.Collections.Generic;

namespace BusinessApp.App
{
    /// <summary>
    /// Add metadata to a query to support filtering, paging etc.
    /// </summary>
    public abstract class Query : IQuery
    {
        public virtual int? Limit { get; set; }
        public virtual int? Offset { get; set; }
        public abstract IEnumerable<string> Sort { get; set; }
        public virtual IEnumerable<string> Embed { get; set; } = new List<string>();
        public virtual IEnumerable<string> Expand { get; set; } = new List<string>();
        public virtual IEnumerable<string> Fields { get; set; } = new List<string>();
    }
}
