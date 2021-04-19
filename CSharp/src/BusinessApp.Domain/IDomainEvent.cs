using System;

namespace BusinessApp.Domain
{
    /// <summary>
    /// Represents event data
    /// </summary>
    public interface IDomainEvent
    {
        DateTimeOffset OccurredUtc { get; }
    }
}
