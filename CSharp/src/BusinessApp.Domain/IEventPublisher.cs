namespace BusinessApp.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to publish events
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync(IEventEmitter emitter, CancellationToken cancelToken);
    }
}
