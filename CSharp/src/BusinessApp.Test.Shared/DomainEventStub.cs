using System;
using BusinessApp.Domain;
using FakeItEasy;

namespace BusinessApp.Test.Shared
{
    public class DomainEventStub : IDomainEvent
    {
        public IEntityId Id { get; set; }
        public DateTimeOffset OccurredUtc { get; set; } = A.Dummy<DateTimeOffset>();

        public string ToString(string format, IFormatProvider formatProvider) => "";
    }
}

