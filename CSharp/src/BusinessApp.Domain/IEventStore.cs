namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface to persist an event
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Adds an event to the store
        /// </summary>
        EventTrackingId Add<T>(T @event) where T : notnull, IDomainEvent;
    }
}
