using System;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Represents event data
    /// </summary>
    public interface IEvent
    {
        DateTimeOffset OccurredUtc { get; }
    }
}
