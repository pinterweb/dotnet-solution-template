namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Authorizes a user issuing a command based on the <see cref="AuthorizeAttribute" />
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
            this.decoratedHandler = GuardAgainst.Null(decoratedHandler, nameof(decoratedHandler));
            this.authorizer = GuardAgainst.Null(authorizer, nameof(authorizer));
        }

        public Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            authorizer.AuthorizeObject(command);

            return decoratedHandler.HandleAsync(command, cancellationToken);
        }
    }
}
