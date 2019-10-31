namespace BusinessApp.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface to publish events from an object
    /// </summary>
    public interface IEventEmitter
    {
        IEnumerable<IDomainEvent> PublishEvents();
    }
}
