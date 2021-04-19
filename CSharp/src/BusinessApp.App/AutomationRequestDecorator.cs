using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    /// <summary>
    /// Automates workflows using the <see cref="IProcessManager" /> based on
    /// events
    /// </summary>
    public class AutomationRequestDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : ICompositeEvent
    {
        private readonly IProcessManager manager;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public AutomationRequestDecorator(IRequestHandler<TRequest, TResponse> inner, IProcessManager manager)
        {
            this.manager = manager.NotNull().Expect(nameof(manager));
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            return await inner.HandleAsync(request, cancelToken)
                .AndThenAsync(r => TriggerAutomation(r, cancelToken));
        }

        private async Task<Result<TResponse, Exception>> TriggerAutomation(TResponse response,
            CancellationToken cancelToken)
        {
            return await manager.HandleNextAsync(response.Events, cancelToken)
                .MapOrElseAsync(
                    e => Result.Error<TResponse>(e),
                    v => Result.Ok(response));
        }
    }
}
