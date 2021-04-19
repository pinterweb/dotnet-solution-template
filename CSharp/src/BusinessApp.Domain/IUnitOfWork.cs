using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface for the Unit of work pattern
    /// </summary>
    public interface IUnitOfWork
    {
        event EventHandler Committing;
        event EventHandler Committed;
        T? Find<T>(Func<T, bool> filter) where T : AggregateRoot;
        void Add<T>(T aggregate) where T : AggregateRoot;
        void Remove<T>(T aggregate) where T : AggregateRoot;
        void Track<T>(T aggregate) where T : AggregateRoot;
        Task CommitAsync(CancellationToken cancelToken);
        Task RevertAsync(CancellationToken cancelToken);
    }
}
