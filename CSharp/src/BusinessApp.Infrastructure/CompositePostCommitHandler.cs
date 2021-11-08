using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Runs multiple post commit handlers
    /// </summary>
    public class CompositePostCommitHandler<TRequest, TResponse> :
        IPostCommitHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IPostCommitHandler<TRequest, TResponse>> handlers;

        public CompositePostCommitHandler(IEnumerable<IPostCommitHandler<TRequest, TResponse>> handlers)
        {
            this.handlers = handlers.NotNull().Expect(nameof(handlers));
        }

        public async Task<Result<Unit, Exception>> HandleAsync(TRequest request,
            TResponse response, CancellationToken cancelToken)
        {
            var results = new List<Result<Unit, Exception>>();

            foreach (var handler in handlers)
            {
                results.Add(await handler.HandleAsync(request, response, cancelToken));
            }

            return results.Collect().AndThen(ok => Result.Ok());
        }
    }
}
