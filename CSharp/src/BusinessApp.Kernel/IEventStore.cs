namespace BusinessApp.Kernel
{
    /// <summary>
    /// Interface to persist an event
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Adds an event to the store
        /// </summary>
        EventTrackingId Add<T>(T e) where T : notnull, IDomainEvent;
    }
}
