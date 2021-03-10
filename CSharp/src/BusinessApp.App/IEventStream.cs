namespace BusinessApp.App
{
    using System.Collections.Generic;
    using BusinessApp.Domain;

    /// <summary>
    /// Contract for objects that have events
    /// </summary>
    public interface IEventStream
    {
        IEnumerable<IDomainEvent> Events { get; }
    }
}
