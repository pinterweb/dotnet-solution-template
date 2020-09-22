﻿namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Authorizes a user based on the <typeparam name="TCommand">TCommand</typeparam>
    /// </summary>
    public class AuthorizationCommandDecorator<TCommand> : ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> decoratedHandler;
        private readonly IAuthorizer<TCommand> authorizer;

        public AuthorizationCommandDecorator(
            ICommandHandler<TCommand> decoratedHandler,
            IAuthorizer<TCommand> authorizer
        )
        {
            this.decoratedHandler = Guard.Against.Null(decoratedHandler).Expect(nameof(decoratedHandler));
            this.authorizer = Guard.Against.Null(authorizer).Expect(nameof(authorizer));
        }

        public Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            authorizer.AuthorizeObject(command);

            return decoratedHandler.HandleAsync(command, cancellationToken);
        }
    }
}
