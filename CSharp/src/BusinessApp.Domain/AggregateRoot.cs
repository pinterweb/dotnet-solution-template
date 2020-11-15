namespace BusinessApp.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The root entity within an aggregate, without an exposed Id
    /// </summary>
    public abstract class AggregateRoot : IEventEmitter
    {
        protected ICollection<IDomainEvent> Events { get; }
            = new List<IDomainEvent>();

        public bool HasEvents() => Events.Count > 0;

        public IEnumerable<IDomainEvent> PublishEvents()
        {
            var events = Events.ToArray();
            Events.Clear();

            return events;
        }
    }
}
