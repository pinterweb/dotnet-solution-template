namespace BusinessApp.App
{
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Authorizes a user issuing a command based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public class AuthorizationCommandDecorator<TCommand> :
        AuthorizeAttributeHandler<TCommand>,
        ICommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> decoratedHandler;

        public AuthorizationCommandDecorator(
            ICommandHandler<TCommand> decoratedHandler,
            IPrincipal currentUser,
            ILogger logger
        )
            : base(currentUser, logger)
        {
            this.decoratedHandler = decoratedHandler;
        }

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            if (Attribute != null)
            {
                Authorize(typeof(TCommand).Name);
            }

            await decoratedHandler.HandleAsync(command, cancellationToken);
        }
    }
}
