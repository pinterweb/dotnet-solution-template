namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    public class SimpleInjectorAsyncScopeCommandProxy<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly Container container;
        private readonly Func<ICommandHandler<IEnumerable<TCommand>>> factory;

        public SimpleInjectorAsyncScopeCommandProxy(Container container,
            Func<ICommandHandler<IEnumerable<TCommand>>> factory)
        {
            this.container = GuardAgainst.Null(container, nameof(container));
            this.factory = GuardAgainst.Null(factory, nameof(factory));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command, CancellationToken cancellationToken)
        {
            using (var scope = AsyncScopedLifestyle.BeginScope(container))
            {
                var inner = factory();
                await inner.HandleAsync(command, cancellationToken);
            }
        }
    }
}
