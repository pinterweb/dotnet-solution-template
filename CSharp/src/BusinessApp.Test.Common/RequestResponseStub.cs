namespace BusinessApp.Test
{
    using BusinessApp.App;

    public class EnvelopeRequestStub : Query, IQuery<EnvelopeContract<ResponseStub>>
    {
    }

    public class ResponseStub
    {
        public int Id { get; set; }
    }
}
