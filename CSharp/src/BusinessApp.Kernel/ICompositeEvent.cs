using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    public interface ICompositeEvent
    {
        IEnumerable<IDomainEvent> Events { get; set; }
    }
}
