namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class ApplicationScopeBatchDecorator<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, TResponse>
    {
        private readonly IAppScope scope;
        private readonly Func<IRequestHandler<IEnumerable<TRequest>, TResponse>> factory;

        public ApplicationScopeBatchDecorator(IAppScope scope,
            Func<IRequestHandler<IEnumerable<TRequest>, TResponse>> factory)
        {
            this.scope = Guard.Against.Null(scope).Expect(nameof(scope));
            this.factory = Guard.Against.Null(factory).Expect(nameof(factory));
        }

        public Task<Result<TResponse, IFormattable>> HandleAsync(
            IEnumerable<TRequest> request,
            CancellationToken cancellationToken)
        {
            using (var _ = scope.NewScope())
            {
                var inner = factory();
                return inner.HandleAsync(request, cancellationToken);
            }
        }
    }
}
