using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service to map an event to a request
    /// </summary>
    /// <remarks>
    /// Useful when you are automating processes that result from events
    /// </remarks>
    public interface IRequestMapper<TRequest, TEvent>
        where TRequest : notnull
        where TEvent : IDomainEvent
    {
        void Map(TRequest request, TEvent e);
    }
}
