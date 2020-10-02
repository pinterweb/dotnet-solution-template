namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchCommandGroupDecorator<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>>
    {
        private static readonly Regex regex = new Regex(@"^\[(\d+)\](\..*)$");
        private readonly IBatchGrouper<TRequest> grouper;
        private readonly IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>> handler;

        public BatchCommandGroupDecorator(
            IBatchGrouper<TRequest> grouper,
            IRequestHandler<IEnumerable<TRequest>, IEnumerable<TResponse>> handler)
        {
            this.grouper = Guard.Against.Null(grouper).Expect(nameof(grouper));
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task<Result<IEnumerable<TResponse>, IFormattable>> HandleAsync(
            IEnumerable<TRequest> request,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(request).Expect(nameof(request));

            var payloads = await grouper.GroupAsync(request, cancellationToken);

            var tasks = new List<(IEnumerable<TRequest>, Task<Result<IEnumerable<TResponse>, IFormattable>>)>();

            foreach (var group in payloads)
            {
                tasks.Add((group, handler.HandleAsync(group, cancellationToken)));
            }

            var _ = await Task.WhenAll(tasks.Select(s => s.Item2));

            var orderedResults = new List<Result<IEnumerable<TResponse>, IFormattable>>();

            foreach (var item in request)
            {
                var target = tasks.Single(t => t.Item1.Contains(item));

                orderedResults.Add(target.Item2.Result);
            }

            if (orderedResults.Any(r => r.Kind == Result.Error))
            {
                return Result<IEnumerable<TResponse>, IFormattable>
                    .Error(new BatchException(
                        orderedResults.Select(o => o.IgnoreValue()
                    )));
            }

            return Result<IEnumerable<TResponse>, IFormattable>
                .Ok(orderedResults.SelectMany(o => o.Unwrap()));
        }
    }
}
