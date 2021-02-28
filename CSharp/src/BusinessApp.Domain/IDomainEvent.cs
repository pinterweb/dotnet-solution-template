namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Represents event data
    /// </summary>
    public interface IDomainEvent
    {
        IEntityId Id { get; set; }
        DateTimeOffset OccurredUtc { get; }
    }
}
