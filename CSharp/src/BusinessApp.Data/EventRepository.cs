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
            this.db = Guard.Against.Null(db).Expect(nameof(db));
            this.user = Guard.Against.Null(user).Expect(nameof(user));
            correlationId = new EventId();
        }

        public void Add(IDomainEvent @event)
        {
            Guard.Against.Null(@event).Expect(nameof(@event));

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
