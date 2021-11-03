using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Runs multiple the post commit handler for batch requests
    /// </summary>
    public class BatchPostCommitAdapter<TRequest, TResponse> :
        IPostCommitHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
        where TRequest : notnull
    {
        private readonly IPostCommitHandler<TRequest, TResponse> inner;

        public BatchPostCommitAdapter(IPostCommitHandler<TRequest, TResponse> inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
        }

        public async Task<Result<Unit, Exception>> HandleAsync(IEnumerable<TRequest> request,
            IEnumerable<TResponse> response, CancellationToken cancelToken)
        {
            var allResponses = response;
            var allResults = new List<Result<Unit, Exception>>();

            for (var i = 0; i < response.Count(); i++)
            {
                var nthRequest = request.ElementAt(i);
                var nthResponse = response.ElementAt(i);

                var result = await inner.HandleAsync(nthRequest, nthResponse, cancelToken);

                allResults.Add(result);
            }

            return allResults.Collect().AndThen(ok => Result.Ok());
        }
    }
}
