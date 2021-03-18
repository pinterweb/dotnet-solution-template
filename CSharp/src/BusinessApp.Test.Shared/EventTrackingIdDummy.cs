namespace BusinessApp.Test.Shared
{
    using BusinessApp.Domain;
    using FakeItEasy;

    public class EventTrackingIdDummy : DummyFactory<EventTrackingId>
    {
        protected override EventTrackingId Create()
        {
            return new EventTrackingId(A.Dummy<MetadataId>(), A.Dummy<MetadataId>());
        }
    }
}
