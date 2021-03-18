namespace BusinessApp.Domain
{
    using System.Collections.Generic;

    public interface IEventStream
    {
        IEnumerable<IDomainEvent> Events { get; set; }
    }
}
