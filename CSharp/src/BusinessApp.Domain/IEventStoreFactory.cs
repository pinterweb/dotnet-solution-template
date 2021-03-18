namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface to create an <see cref="IEventStore" />
    /// </summary>
    public interface IEventStoreFactory
    {
        /// <summary>
        /// Creates an event store from an event trigger
        /// </summary>
        IEventStore Create<T>(T eventTrigger) where T : class;
    }
}
