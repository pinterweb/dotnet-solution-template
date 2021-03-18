namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface to create an event publisher
    /// </summary>
    public interface IEventPublisherFactory
    {
        /// <summary>
        /// Creates an <see cref="IEventPublisher" />, using the <param name="caller"></param>
        /// as the originator of the events
        /// </summary>
        IEventPublisher Create<T>(T trigger) where T : class;
    }
}
