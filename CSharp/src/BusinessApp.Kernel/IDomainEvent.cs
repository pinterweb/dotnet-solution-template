using System;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Represents event data
    /// </summary>
    public interface IDomainEvent
    {
        DateTimeOffset OccurredUtc { get; }
    }
}
