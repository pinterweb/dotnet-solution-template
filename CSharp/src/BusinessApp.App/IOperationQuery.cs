namespace BusinessApp.App
{
    using System.Collections.Generic;

    /// <summary>
    /// Adds support for different operator types such as greater than, less than etc.
    /// </summary>
    public interface IOperationQuery<T>
    {
        string Operator { get; set; }
        IEnumerable<T> Values { get; set; }
    }
}
