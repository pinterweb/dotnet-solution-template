namespace BusinessApp.Data
{
    using BusinessApp.Domain;

    /// <summary>
    /// Repository to keep track of the events emitted
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly BusinessAppDbContext db;

        public EventRepository(BusinessAppDbContext db)
        {
            this.db = GuardAgainst.Null(db, nameof(db));
        }

        public void Add(IDomainEvent @event)
        {
            GuardAgainst.Null(@event, nameof(@event));
            db.Add(@event);
        }
    }
}
