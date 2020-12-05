namespace BusinessApp.Test
{
    using System;
    using BusinessApp.Domain;
    using FakeItEasy;

    public class DomainEventStub : IDomainEvent
    {
        public IEntityId Id { get; set; }
        public DateTimeOffset OccurredUtc => A.Dummy<DateTimeOffset>();

        public string ToString(string format, IFormatProvider formatProvider) => "";
    }
}

