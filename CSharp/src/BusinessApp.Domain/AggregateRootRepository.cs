namespace BusinessApp.Domain
{
    /// <summary>
    /// A simple abstract repository handling any <see cref="AggregateRoot"/>
    /// </summary>
    public abstract class AggregateRootRepository<TAggregate> :
        IAggregateRootRepository<TAggregate>
            where TAggregate : AggregateRoot
    {
        private readonly IUnitOfWork uow;

        public AggregateRootRepository(IUnitOfWork uow)
        {
            this.uow = Guard.Against.Null(uow).Expect(nameof(uow));
        }

        public virtual void Add(TAggregate aggregate) => uow.Add(aggregate);

        public virtual void Delete(TAggregate aggregate) => uow.Remove(aggregate);
    }
}
