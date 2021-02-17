namespace BusinessApp.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the Unit of work pattern
    /// </summary>
    public interface IUnitOfWork
    {
        event EventHandler Committing;
        event EventHandler Committed;
        TRoot Find<TRoot>(Func<TRoot, bool> filter) where TRoot : AggregateRoot;
        void Add(AggregateRoot aggregate);
        void Add(IDomainEvent @event);
        void Remove(AggregateRoot aggregate);
        void Track(AggregateRoot aggregate);
        Task CommitAsync(CancellationToken cancelToken);
        Task RevertAsync(CancellationToken cancelToken);
    }
}
