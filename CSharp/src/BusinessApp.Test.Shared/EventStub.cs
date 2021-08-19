using System;
using BusinessApp.Kernel;
using FakeItEasy;

namespace BusinessApp.Test.Shared
{
    public class EventStub : IEvent
    {
        public IEntityId Id { get; set; }
        public DateTimeOffset OccurredUtc { get; set; } = A.Dummy<DateTimeOffset>();

        public string ToString(string format, IFormatProvider formatProvider) => "";
    }
}
