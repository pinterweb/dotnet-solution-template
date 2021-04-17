namespace BusinessApp.Domain
{
    using System.Collections.Generic;

    public interface ICompositeEvent
    {
        IEnumerable<IDomainEvent> Events { get; set; }
    }
}
