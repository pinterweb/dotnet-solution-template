namespace BusinessApp.WebApi
{
    using System;
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
            this.container = container.NotNull().Expect(nameof(container));
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
                    var handler = (EventHandler)Activator.CreateInstance(
                        typeof(GenericEventHandler<>).MakeGenericType(@event.GetType()));

                    await handler.HandleAsync(@event, cancellationToken, container);
                }

                events = emitter.PublishEvents();
            }
        }

        private abstract class EventHandler
        {
            public abstract Task HandleAsync(IDomainEvent request,
                CancellationToken cancellationToken,
                Container container);
        }

        private class GenericEventHandler<TEvent> : EventHandler
              where TEvent : IDomainEvent
        {
            public async override Task HandleAsync(IDomainEvent @event,
                CancellationToken cancellationToken,
                Container container)
            {
                var handlers =  container.GetAllInstances<IEventHandler<TEvent>>();

                foreach (var handler in handlers)
                {
                    await handler.HandleAsync((TEvent)@event, cancellationToken);
                }
            }
        }
    }
}
