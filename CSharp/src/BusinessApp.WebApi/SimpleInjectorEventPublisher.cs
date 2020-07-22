namespace BusinessApp.WebApi
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using SimpleInjector;

    /// <summary>
    /// SimpleInjector implementation of the <see cref="IEventPublisher"/>
    /// </summary>
    public class SimpleInjectorEventPublisher : IEventPublisher
    {
        private readonly Container container;

        public SimpleInjectorEventPublisher(Container container)
        {
            this.container = GuardAgainst.Null(container, nameof(container));
        }

        /// <summary>
        /// Handle all the events stored in the <see cref="IEventEmitter"/>
        /// </summary>
        /// <param name="emmitter"></param>
        /// <returns></returns>
        public async Task PublishAsync(IEventEmitter emitter, CancellationToken cancellationToken)
        {
            var events = emitter.PublishEvents();

            // if event handlers create more events then loop over those too
            while (events.Count() > 0)
            {
                foreach (var @event in events)
                {
                    var handlers = container.GetAllInstances(typeof(IEventHandler<>).MakeGenericType(@event.GetType()));

                    foreach (dynamic handler in handlers)
                    {
                        await handler.HandleAsync((dynamic)@event, cancellationToken);
                    }
                }

                events = emitter.PublishEvents();
            }
        }
    }
}
