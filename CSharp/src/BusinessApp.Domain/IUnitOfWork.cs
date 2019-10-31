namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for the Unit of work pattern
    /// </summary>
    public interface IUnitOfWork
    {
        void Add(AggregateRoot aggregate);
        void Remove(AggregateRoot aggregate);
        Task CommitAsync(CancellationToken cancellationToken);
        Task RevertAsync(CancellationToken cancellationToken);
    }
}
