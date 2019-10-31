namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Represents event data
    /// </summary>
    public interface IDomainEvent
    {
        DateTimeOffset Occurred { get; }
    }
}
