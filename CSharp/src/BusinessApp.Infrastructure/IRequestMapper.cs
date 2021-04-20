using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    public interface IRequestMapper<TRequest, TEvent>
        where TRequest : notnull
        where TEvent : IDomainEvent
    {
        void Map(TRequest request, TEvent @event);
    }
}
