using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessApp.Infrastructure
{
    public interface IRequestStore
    {
        Task<IEnumerable<RequestMetadata>> GetAllAsync();
    }
}
