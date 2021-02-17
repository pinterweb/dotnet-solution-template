namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to handle event logic
    /// </summary>
    public interface IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancelToken);
    }
}
