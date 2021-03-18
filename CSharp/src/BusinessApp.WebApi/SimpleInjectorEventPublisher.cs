using BusinessApp.Domain;
using System;
using System.Collections.Generic;

namespace BusinessApp.WebApi
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleInjector;

    using EventResult = Result<IEnumerable<IDomainEvent>, Exception>;

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

        public Task<EventResult> PublishAsync<T>(T @event, CancellationToken cancelToken)
            where T : IDomainEvent
        {
            var handler = (EventHandler)Activator.CreateInstance(
                typeof(GenericEventHandler<>).MakeGenericType(typeof(T)));

            return handler.HandleAsync(@event, cancelToken, container);
        }

        private abstract class EventHandler
        {
            public abstract Task<EventResult> HandleAsync(IDomainEvent request,
                CancellationToken cancelToken, Container container);
        }

        private class GenericEventHandler<TEvent> : EventHandler
              where TEvent : IDomainEvent
        {
            public async override Task<EventResult> HandleAsync(IDomainEvent @event,
                CancellationToken cancelToken, Container container)
            {
                var handlers =  container.GetAllInstances<IEventHandler<TEvent>>();

                return (
                    await Task.WhenAll(
                        handlers.Select(h => h.HandleAsync((TEvent)@event, cancelToken)))
                    )
                    .Collect()
                    .Map(v => v.SelectMany(s => s));
            }
        }
    }
}
