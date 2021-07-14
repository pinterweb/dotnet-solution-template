using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service to retrieve metadata for a requests
    /// </summary>
    /// <remarks>
    /// Useful when you are automating requets and need to know how to create and
    /// handle the new request
    /// </remarks>
    public interface IRequestStore
    {
        Task<IEnumerable<RequestMetadata>> GetAllAsync();
    }
}
