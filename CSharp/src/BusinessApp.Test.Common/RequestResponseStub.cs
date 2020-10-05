namespace BusinessApp.Test
{
    using System.Collections.Generic;
    using BusinessApp.App;

    public class EnvelopeRequestStub : Query, IQuery<EnvelopeContract<ResponseStub>>
    {
    }

    public class QueryStub : Query, IQuery<ResponseStub>
    {
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
