namespace BusinessApp.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;

    public interface IDatastore<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> QueryAsync(IQuery query, CancellationToken cancellationToken);
    }
}
