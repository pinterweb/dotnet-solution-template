namespace BusinessApp.Data
{
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// Repository to keep track of the events emitted
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly EventId correlationId;
        private readonly IEntityIdFactory<EventId> idFactory;

        public EventRepository(BusinessAppDbContext db, IPrincipal user,
            IEntityIdFactory<EventId> idFactory)
        {
            this.db = db.NotNull().Expect(nameof(db));
            this.user = user.NotNull().Expect(nameof(user));
            this.idFactory = idFactory.NotNull().Expect(nameof(idFactory));
            correlationId = idFactory.Create();
        }

        public void Add<T>(T @event) where T : IDomainEvent
        {
            @event.NotNull().Expect(nameof(@event));

            var id = idFactory.Create();
            @event.Id = id;

            db.Add(new EventMetadata(@event, correlationId, user.Identity.Name));

            db.Add(@event);
        }
    }
}
