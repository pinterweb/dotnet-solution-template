namespace BusinessApp.Test.Shared
{
    using System.Collections.Generic;
    using BusinessApp.App;

    public class EnvelopeRequestStub : IQuery
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public IEnumerable<string> Sort { get; set; }
        public IEnumerable<string> Embed { get; set; }
        public IEnumerable<string> Expand { get; set; }
        public IEnumerable<string> Fields { get; set; }
    }

    public class QueryStub : IQuery
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public IEnumerable<string> Sort { get; set; }
        public IEnumerable<string> Embed { get; set; }
        public IEnumerable<string> Expand { get; set; }
        public IEnumerable<string> Fields { get; set; }
    }

    public class RequestStub
    {
        public int Id { get; set; }
    }

    public class ResponseStub
    {
        public int Id { get; set; }
        public IEnumerable<ResponseStub> ChildResponseStubs { get; set; }
    }

    public class ChildResponseStub
    {
        public int Id { get; set; }
        public int ResponseStubId { get; set; }
        public ResponseStub ResponseStub { get; set; }
    }
}
