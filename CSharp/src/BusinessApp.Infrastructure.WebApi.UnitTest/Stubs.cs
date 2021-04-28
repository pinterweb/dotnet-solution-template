using System;
using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.WebApi.UnitTest
{
    public enum EnumQueryStub { Foobar }

    public class EventStub : IDomainEvent
    {
        public string Id { get; set; }
        public DateTimeOffset OccurredUtc { get; }
    }

    public class CompoisteEventStub : ICompositeEvent
    {
        public IEnumerable<IDomainEvent> Events { get; set; } = new List<IDomainEvent>();
    }

    public class QueryStub
    {
        public bool? Bool { get; set; }
        public int? Int { get; set; }
        public decimal? Decimal { get; set; }
        public double? Double { get; set; }
        public string Foo { get; set; }
        public string Lorem { get; set; }
        public DateTime? DateTime { get; set; }
        public EnumQueryStub? Enum { get; set; }
        public NestedQueryStub SingleNested { get; set; }
        public DeeplyNestedQueryStub DeeplyNested { get; set; }
        public IEnumerable<NestedQueryStub> MultiNested { get; set; }
        public IEnumerable<float> Enumerable { get; set; }

        public class NestedQueryStub
        {
            public string Ipsit { get; set; }
            public string Dolor { get; set; }
            public IEnumerable<float> Enumerable { get; set; }
        }

        public class DeeplyNestedQueryStub
        {
            public NestedQueryStub Nested { get; set; }
        }
    }
}
