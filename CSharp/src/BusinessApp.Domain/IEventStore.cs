namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface to persist a stream of events
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Adds an event to the store
        /// </summary>
        // void Add<T>(IEventOriginator<T> @event);
        void Add(IEventStream stream);
    }
}
