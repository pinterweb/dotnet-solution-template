using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// SimpleInjector implementation of the <see cref="IProcessManager"/>
    /// </summary>
    public class SimpleInjectorProcessManager : IProcessManager
    {
        private readonly Container container;
        private readonly IRequestStore store;

        public SimpleInjectorProcessManager(Container container, IRequestStore store)
        {
            this.container = container.NotNull().Expect(nameof(container));
            this.store = store.NotNull().Expect(nameof(store));
        }

        public async Task<Result<Unit, Exception>> HandleNextAsync(IEnumerable<IEvent> events,
            CancellationToken cancelToken)
        {
            var commands = await store.GetAllAsync();

            if (!commands.Any()) return Result.Ok(Unit.Value);

            var tasks = new List<Task<Result<Unit, Exception>>>();

            var eventTypes = events.Select(e => e.GetType());

            foreach (var command in commands)
            {
                var handler = (RequestHandler)Activator.CreateInstance(
                    typeof(GenericRequestHandler<,>)
                        .MakeGenericType(command.RequestType, command.ResponseType))!;

                var request = Activator.CreateInstance(command.RequestType)!;

                foreach (var e in events.Where(e => command.EventTriggers.Contains(e.GetType())))
                {
                    var mapper = (CommandMapper)Activator.CreateInstance(
                        typeof(CommandMapper<,>)
                            .MakeGenericType(command.RequestType, e.GetType()))!;

                    mapper.Map(request, e, container);
                    tasks.Add(handler.HandleAsync(request, container, cancelToken));
                }
            }

            return await Task.WhenAll(tasks)
                .CollectAsync()
                .MapAsync(r => Unit.Value);
        }

        private abstract class CommandMapper
        {
            public abstract void Map(object request, IEvent @event, Container container);
        }

        private class CommandMapper<R, E> : CommandMapper
            where R : notnull, new()
            where E : IEvent
        {
            public override void Map(object request, IEvent @event, Container container)
            {
                var mapper = container.GetInstance<IRequestMapper<R, E>>();

                mapper.Map((R)request, (E)@event);
            }
        }

        private abstract class RequestHandler
        {
            public abstract Task<Result<Unit, Exception>> HandleAsync(object request,
                Container container, CancellationToken cancelToken);
        }

        private class GenericRequestHandler<T, R> : RequestHandler
            where T : notnull
        {
            public override Task<Result<Unit, Exception>> HandleAsync(object request,
                Container container, CancellationToken cancelToken)
                => HandleAsync((T)request, container, cancelToken)
                    .MapAsync(_ => Unit.Value);

            public static Task<Result<R, Exception>> HandleAsync(T request,
                Container container, CancellationToken cancelToken)
            {
                var handler = container.GetInstance<IRequestHandler<T, R>>();

                return handler.HandleAsync(request, cancelToken);
            }
        }
    }
}
