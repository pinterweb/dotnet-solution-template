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
            this.db = Guard.Against.Null(db).Expect(nameof(db));
        }

        public void Add(IDomainEvent @event)
        {
            Guard.Against.Null(@event).Expect(nameof(@event));

            db.Add(@event);
        }
    }
}
