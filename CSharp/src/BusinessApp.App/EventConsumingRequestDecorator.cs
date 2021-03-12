namespace BusinessApp.App
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    /// <summary>
    /// Consume event data from the prior handlers
    /// </summary>
    public class EventConsumingRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TResponse : IEventStream
    {
        private readonly IEventPublisherFactory publisherFactory;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public EventConsumingRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IEventPublisherFactory publisherFactory)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.publisherFactory = publisherFactory.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            var streamResult = await inner.HandleAsync(request, cancelToken);

            var publisher = publisherFactory.Create(request);

            return (await streamResult
                .Map(s => s.Events)
                .AndThenAsync((events, ct) => ConsumeAsync(publisher, events, ct), cancelToken))
                .MapOrElse(
                    err => Result.Error<TResponse>(err),
                    ok =>
                    {
                        streamResult.Unwrap().Events = ok;
                        return streamResult;
                    }
                );
        }

        private async Task<Result<IEnumerable<IDomainEvent>, Exception>> ConsumeAsync(
            IEventPublisher publisher, IEnumerable<IDomainEvent> events, CancellationToken cancelToken)
        {
            var consumedEvents = events.Select(s => s).ToList();

            if (!consumedEvents.Any()) return Result.Ok<IEnumerable<IDomainEvent>>(consumedEvents);

            return
            (
                await
                (
                    await Task.WhenAll(consumedEvents.Select(e => publisher.PublishAsync(e, cancelToken)))
                )
                .Collect()
                .Map(v => v.SelectMany(s => s))
                .AndThenAsync((events, ct) => ConsumeAsync(publisher, events, ct), cancelToken)
            )
                .Map(e => consumedEvents.Concat(e));
        }
    }
}
