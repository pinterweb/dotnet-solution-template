using System.Collections.Generic;

namespace BusinessApp.Domain
{
    public interface ICompositeEvent
    {
        IEnumerable<IDomainEvent> Events { get; set; }
    }
}
