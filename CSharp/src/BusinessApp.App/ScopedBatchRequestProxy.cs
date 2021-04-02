namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class ScopedBatchRequestProxy<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, TResponse>
        where TRequest : notnull
    {
        private readonly IAppScope scope;
        private readonly Func<IRequestHandler<IEnumerable<TRequest>, TResponse>> factory;

        public ScopedBatchRequestProxy(IAppScope scope,
            Func<IRequestHandler<IEnumerable<TRequest>, TResponse>> factory)
        {
            this.scope = scope.NotNull().Expect(nameof(scope));
            this.factory = factory.NotNull().Expect(nameof(factory));
        }

        public Task<Result<TResponse, Exception>> HandleAsync(IEnumerable<TRequest> request,
            CancellationToken cancelToken)
        {
            using (var _ = scope.NewScope())
            {
                var inner = factory();
                return inner.HandleAsync(request, cancelToken);
            }
        }
    }
}
