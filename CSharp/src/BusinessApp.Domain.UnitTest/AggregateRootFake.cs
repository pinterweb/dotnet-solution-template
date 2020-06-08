namespace BusinessApp.Domain.UnitTest
{
    using FakeItEasy;

    public class AggregateRootFake : AggregateRoot
    {
        public void AddEvent(IDomainEvent @event = null)
        {
            Events.Add(@event ?? A.Dummy<IDomainEvent>());
        }

        public void ClearEvents(IDomainEvent @event = null)
        {
            if (@event != null)
            {
                Events.Remove(@event);
            }
            else
            {
                Events.Clear();
            }
        }
    }
}
