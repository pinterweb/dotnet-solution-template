namespace BusinessApp.App
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRequestStore
    {
        Task<IEnumerable<RequestMetadata>> GetAllAsync();
    }
}
