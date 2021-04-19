using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessApp.App
{
    public interface IRequestStore
    {
        Task<IEnumerable<RequestMetadata>> GetAllAsync();
    }
}
