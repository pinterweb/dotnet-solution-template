namespace BusinessApp.WebApi.UnitTest
{
    using System;
    using System.Collections.Generic;

    public enum EnumQueryStub { Foobar }

    public class NestedQueryStub
    {
        public string Ipsit { get; set; }
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
        public IEnumerable<NestedQueryStub> MultiNested { get; set; }
        public IEnumerable<float> Enumerable { get; set; }
    }
}
