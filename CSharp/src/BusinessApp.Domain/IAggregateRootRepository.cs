namespace BusinessApp.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Repository Pattern for commands against the aggregate
    /// </summary>
    public interface IAggregateRootRepository<TAggregate>
        where TAggregate : AggregateRoot
    {
        void Add(TAggregate aggregate);
        void Delete(TAggregate aggregate);
    }

    /// <summary>
    /// Repository Pattern for a <see cref="AggregateRoot{TId}"/>
    /// </summary>
    public interface IAggregateRootRepository<TAggregate, TId> :
        IAggregateRootRepository<TAggregate>
        where TAggregate : AggregateRoot<TId>
        where TId : IEntityId
    {
        Task<IEnumerable<TAggregate>> GetAllAsync(IEnumerable<TId> ids);
        Task<TAggregate> GetByIdAsync(TId id);
    }
}
