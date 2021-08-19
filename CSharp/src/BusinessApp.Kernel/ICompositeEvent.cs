using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Data holding many events
    /// </summary>
    public interface ICompositeEvent
    {
        IEnumerable<IEvent> Events { get; set; }
    }
}
