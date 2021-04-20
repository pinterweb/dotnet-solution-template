using BusinessApp.Kernel;
using FakeItEasy;

namespace BusinessApp.Test.Shared
{
    public class EventTrackingIdDummy : DummyFactory<EventTrackingId>
    {
        protected override EventTrackingId Create()
        {
            return new EventTrackingId(A.Dummy<MetadataId>(), A.Dummy<MetadataId>());
        }
    }
}
