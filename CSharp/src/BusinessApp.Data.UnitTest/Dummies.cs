namespace BusinessApp.Data.UnitTest
{
    using BusinessApp.App;

    public class DummyResponse {}

    public class DummyRequest : Query, IQuery<EnvelopeContract<DummyResponse>> { }
}

