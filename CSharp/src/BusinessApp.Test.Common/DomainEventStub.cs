namespace BusinessApp.Test
{
    using System;
    using BusinessApp.Domain;
    using FakeItEasy;

    public class DomainEventStub : IDomainEvent
    {
        public int Id { get; set; }
        public DateTimeOffset Occurred => A.Dummy<DateTimeOffset>();
    }
}

