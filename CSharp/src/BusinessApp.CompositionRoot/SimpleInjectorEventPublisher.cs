using BusinessApp.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
#pragma warning disable IDE0065
    using EventResult = Result<IEnumerable<IEvent>, Exception>;
#pragma warning restore IDE0065

    /// <summary>
    /// SimpleInjector implementation of the <see cref="IEventPublisher"/>
    /// </summary>
    public class SimpleInjectorEventPublisher : IEventPublisher
    {
        private readonly Container container;

        public SimpleInjectorEventPublisher(Container container)
            => this.container = container.NotNull().Expect(nameof(container));

        public Task<EventResult> PublishAsync<T>(T e, CancellationToken cancelToken)
            where T : notnull, IEvent
        {
            var handler = (EventHandler)Activator.CreateInstance(
                typeof(GenericEventHandler<>).MakeGenericType(typeof(T)))!;

            return handler.HandleAsync(e, container, cancelToken);
        }

        private abstract class EventHandler
        {
            public abstract Task<EventResult> HandleAsync(IEvent request,
                Container container, CancellationToken cancelToken);
        }

        private class GenericEventHandler<TEvent> : EventHandler
              where TEvent : IEvent
        {
            public override async Task<EventResult> HandleAsync(IEvent @event,
                Container container, CancellationToken cancelToken)
            {
                var handlers = container.GetAllInstances<IEventHandler<TEvent>>();

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
