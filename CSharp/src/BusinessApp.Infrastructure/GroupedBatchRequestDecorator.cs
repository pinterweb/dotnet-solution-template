using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Groups related requests together before running any subsequence handlers
    /// </summary>
    /// <remarks>
    /// This is useful for grouping data together in one business transaction
    /// </remarks>
    public class GroupedBatchRequestDecorator<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
        where TRequest : notnull
    {
        private readonly IBatchGrouper<TRequest> grouper;
        private readonly IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>> handler;

        public GroupedBatchRequestDecorator(
            IBatchGrouper<TRequest> grouper,
            IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>> handler)
        {
            this.grouper = grouper.NotNull().Expect(nameof(grouper));
            this.handler = handler.NotNull().Expect(nameof(handler));
        }

        public async Task<Result<IEnumerable<TResponse>, Exception>> HandleAsync(
            IEnumerable<TRequest> request,
            CancellationToken cancelToken)
        {
            var payloads = await grouper.GroupAsync(request, cancelToken);

            ThrowIfInvalid(request, payloads.SelectMany(p => p));

            var tasks = new List<(TRequest[], Task<Result<IEnumerable<TResponse>, Exception>>, TRequest[])>();

            foreach (var group in payloads)
            {
                var originalRequests = request.Intersect(group).ToArray();
                tasks.Add((originalRequests, handler.HandleAsync(group, cancelToken), group.ToArray()));
            }

            var _ = await Task.WhenAll(tasks.Select(s => s.Item2));

            var orderedResults = new List<Result<TResponse, Exception>>();

            for (var i = 0; i < request.Count(); i++)
            {
                var item = request.ElementAt(i);

                var itemTask = tasks.Single(t => t.Item1.Contains(item));

                var groupIndex = Array.IndexOf(tasks
                    .Single(t => t.Item1.Contains(item))
                    .Item1
                    , item);

                var groupItemIndex = Array.IndexOf(tasks
                    .Single(t => t.Item1.Contains(item))
                    .Item3
                    , item);

                var itemResult = tasks.Single(t => t.Item1.Contains(item))
                    .Item2.Result
                    .Map(r => r.ElementAt(groupIndex));

                if (itemResult.Kind == ValueKind.Error && itemResult.UnwrapError() is BatchException be)
                {
                    orderedResults.Add(be.ElementAt(groupItemIndex)
                            .MapOrElse(
                                e => Result.Error<TResponse>(e),
                                o => Result.Ok((TResponse)o)));
                }
                else
                {
                    orderedResults.Add(itemResult);
                }
            }

            return orderedResults.Any(r => r.Kind == ValueKind.Error)
                ? Result.Error<IEnumerable<TResponse>>(BatchException.FromResults(orderedResults))
                : Result.Ok(orderedResults.Select(o => o.Unwrap()));
        }

        private static void ThrowIfInvalid(IEnumerable<TRequest> request,
            IEnumerable<TRequest> groupedRequest)
        {
            if (request.Except(groupedRequest).Any())
            {
                throw new BusinessAppException("Could not find the original " +
                    "command(s) after it was grouped. Consider overriding Equals " +
                    "if the batch grouper creates new classes.");
            }
        }
    }
}
