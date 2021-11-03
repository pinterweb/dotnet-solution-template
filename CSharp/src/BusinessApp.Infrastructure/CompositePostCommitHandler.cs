using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Runs multiple post commit handlers
    /// </summary>
    public class CompositePostCommitHandler<TRequest, TResponse> : IPostCommitHandler<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IPostCommitHandler<TRequest, TResponse>> handlers;

        public CompositePostCommitHandler(IEnumerable<IPostCommitHandler<TRequest, TResponse>> handlers)
        {
            this.handlers = handlers.NotNull().Expect(nameof(handlers));
        }

        public async Task<Result<Unit, Exception>> HandleAsync(TRequest request, TResponse response,
            CancellationToken cancelToken)
        {
            foreach (var handler in handlers)
            {
                var result = await handler.HandleAsync(request, response, cancelToken);

                if (result.Kind == ValueKind.Error)
                {
                    return result;
                }
            }

            return Result.Ok();
        }
    }
}
