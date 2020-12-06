namespace BusinessApp.Data
{
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// Repository to keep track of the events emitted
    /// </summary>
    // TODO move to domain layer
    public class EventRepository : IEventRepository
    {
        private readonly IUnitOfWork db;
        private readonly IPrincipal user;
        private readonly EventId correlationId;

        public EventRepository(IUnitOfWork db,
            IPrincipal user)
        {
            this.db = db.NotNull().Expect(nameof(db));
            this.user = user.NotNull().Expect(nameof(user));
            correlationId = new EventId();
        }

        public void Add(IDomainEvent @event)
        {
            @event.NotNull().Expect(nameof(@event));

            var id = new EventId();
            @event.Id = id;

            db.Add(new EventMetadata(id,
                correlationId,
                @event,
                user.Identity.Name
            ));

            db.Add(@event);
        }
    }
}
