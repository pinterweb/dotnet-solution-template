namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Validates the command prior to handling
    /// </summary>
    public class AutomationRequestDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : IEventStream
    {
        private readonly IProcessManager manager;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public AutomationRequestDecorator(IProcessManager manager, IRequestHandler<TRequest, TResponse> inner)
        {
            this.manager = manager.NotNull().Expect(nameof(manager));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            var result = await inner.HandleAsync(request, cancelToken);

            return await result.AndThenAsync(r => TriggerAutomation(r, cancelToken));
        }

        private async Task<Result<TResponse, Exception>> TriggerAutomation(TResponse response,
            CancellationToken cancelToken)
        {
            var result = await manager.HandleNextAsync(response, cancelToken);

            return result.Kind switch
            {
                ValueKind.Error => Result.Error<TResponse>(result.UnwrapError()),
                ValueKind.Ok => Result.Ok<TResponse>(response),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
