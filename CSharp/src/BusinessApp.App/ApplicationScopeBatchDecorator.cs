namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class ApplicationScopeBatchDecorator<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly IAppScope scope;
        private readonly Func<ICommandHandler<IEnumerable<TCommand>>> factory;

        public ApplicationScopeBatchDecorator(IAppScope scope,
            Func<ICommandHandler<IEnumerable<TCommand>>> factory)
        {
            this.scope = GuardAgainst.Null(scope, nameof(scope));
            this.factory = GuardAgainst.Null(factory, nameof(factory));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command, CancellationToken cancellationToken)
        {
            using (var _ = scope.NewScope())
            {
                var inner = factory();
                await inner.HandleAsync(command, cancellationToken);
            }
        }
    }
}
