namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class GroupedBatchRequestDecorator<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
    {
        private static readonly Regex regex = new Regex(@"^\[(\d+)\](\..*)$");
        private readonly IBatchGrouper<TRequest> grouper;
        private readonly IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>> handler;

        public GroupedBatchRequestDecorator(
            IBatchGrouper<TRequest> grouper,
            IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>> handler)
        {
            this.grouper = grouper.NotNull().Expect(nameof(grouper));
            this.handler = handler.NotNull().Expect(nameof(handler));
        }

        public async Task<Result<IEnumerable<TResponse>, IFormattable>> HandleAsync(
            IEnumerable<TRequest> request,
            CancellationToken cancelToken)
        {
            request.NotNull().Expect(nameof(request));

            var payloads = await grouper.GroupAsync(request, cancelToken);

            var tasks = new List<(IEnumerable<TRequest>, Task<Result<IEnumerable<TResponse>, IFormattable>>)>();

            foreach (var group in payloads)
            {
                tasks.Add((group, handler.HandleAsync(group, cancelToken)));
            }

            var _ = await Task.WhenAll(tasks.Select(s => s.Item2));

            var orderedResults = new List<Result<IEnumerable<TResponse>, IFormattable>>();

            foreach (var item in request)
            {
                var target = tasks.SingleOrDefault(t => t.Item1.Contains(item));

                if (target == default)
                {
                    throw new BusinessAppAppException("Could not find the original command after " +
                        "it was grouped. Consider overriding Equals if the batch grouper " +
                        "creates new classes.");
                }

                orderedResults.Add(target.Item2.Result);
            }

            if (orderedResults.Any(r => r.Kind == ValueKind.Error))
            {
                return Result<IEnumerable<TResponse>, IFormattable>
                    .Error(new BatchException(
                        orderedResults.Select(o => o.Into()
                    )));
            }

            return Result<IEnumerable<TResponse>, IFormattable>
                .Ok(orderedResults.SelectMany(o => o.Unwrap()));
        }
    }
}
