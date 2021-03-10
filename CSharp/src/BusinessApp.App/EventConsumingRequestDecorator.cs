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
        private readonly IEventPublisher publisher;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public EventConsumingRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IEventPublisher publisher)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.publisher = publisher.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            var streamResult = await inner.HandleAsync(request, cancelToken);

            return (await streamResult
                .Map(s => s.Events)
                .AndThenAsync(ConsumeAsync, cancelToken))
                .MapOrElse(
                    err => Result.Error<TResponse>(err),
                    ok => streamResult
                );
        }

        private async Task<Result<IEnumerable<IDomainEvent>, Exception>> ConsumeAsync(
            IEnumerable<IDomainEvent> events, CancellationToken cancelToken)
        {
            if (!events.Any()) return Result.Ok(events);

            return
            (
                await
                (
                    await Task.WhenAll(events.Select(e => publisher.PublishAsync(e, cancelToken)))
                )
                .Collect()
                .Map(v => v.SelectMany(s => s))
                .AndThenAsync(ConsumeAsync, cancelToken)
            );
        }
    }
}
