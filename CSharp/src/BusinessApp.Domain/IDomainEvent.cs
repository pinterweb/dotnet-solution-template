namespace BusinessApp.Domain
{
    using System;

    /// <summary>
    /// Represents event data
    /// </summary>
    public interface IDomainEvent : IFormattable
    {
        IEntityId Id { get; set; }
        DateTimeOffset OccurredUtc { get; }
    }
}
