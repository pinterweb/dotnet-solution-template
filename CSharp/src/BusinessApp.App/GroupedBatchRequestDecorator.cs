namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

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

            var tasks = new List<(IEnumerable<TRequest>, Task<Result<IEnumerable<TResponse>, Exception>>)>();

            foreach (var group in payloads)
            {
                var originalRequests = request.Intersect(group);
                tasks.Add((originalRequests, handler.HandleAsync(group, cancelToken)));
            }

            var _ = await Task.WhenAll(tasks.Select(s => s.Item2));

            var orderedResults = new List<Result<IEnumerable<TResponse>, Exception>>();

            foreach (var item in request)
            {
                var target = tasks.SingleOrDefault(t => t.Item1.Contains(item));
                orderedResults.Add(target.Item2.Result);
            }

            if (orderedResults.Any(r => r.Kind == ValueKind.Error))
            {
                return Result.Error<IEnumerable<TResponse>>(
                    BatchException.FromResults(orderedResults));
            }

            return Result.Ok(orderedResults.SelectMany(o => o.Unwrap()));
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
