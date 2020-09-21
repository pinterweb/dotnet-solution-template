namespace BusinessApp.App
{
    using System.Collections.Generic;

    /// <summary>
    /// Add metadata to a query to support filtering, paging etc.
    /// </summary>
    public abstract class Query
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public IEnumerable<string> Sort { get; set; } = new List<string>();
        public IEnumerable<string> Embed { get; set; } = new List<string>();
        public IEnumerable<string> Expand { get; set; } = new List<string>();
        public IEnumerable<string> Fields { get; set; } = new List<string>();
    }
}
