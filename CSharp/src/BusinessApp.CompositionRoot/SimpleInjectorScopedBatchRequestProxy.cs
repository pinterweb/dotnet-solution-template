using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Create a new set of services for the given scope
    /// </summary>
    public class SimpleInjectorScopedBatchRequestProxy<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, TResponse>
        where TRequest : notnull
    {
        private readonly Container container;
        private readonly Func<IRequestHandler<IEnumerable<TRequest>, TResponse>> factory;

        public SimpleInjectorScopedBatchRequestProxy(Container container,
            Func<IRequestHandler<IEnumerable<TRequest>, TResponse>> factory)
        {
            this.container = container.NotNull().Expect(nameof(container));
            this.factory = factory.NotNull().Expect(nameof(factory));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(IEnumerable<TRequest> request,
            CancellationToken cancelToken)
        {
            using var _ = AsyncScopedLifestyle.BeginScope(container);

            var inner = factory();

            return await inner.HandleAsync(request, cancelToken);
        }
    }
}
