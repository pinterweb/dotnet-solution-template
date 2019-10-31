namespace BusinessApp.App
{
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Authorizes a user issuing a query based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public class AuthorizationQueryDecorator<TQuery, TResult> :
        AuthorizeAttributeHandler<TQuery>,
        IQueryHandler<TQuery, TResult>
           where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> decoratedHandler;

        public AuthorizationQueryDecorator(
            IQueryHandler<TQuery, TResult> decoratedHandler,
            IPrincipal currentUser,
            ILogger logger
        )
            : base(currentUser, logger)
        {
            this.decoratedHandler = decoratedHandler;
        }

        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationtoken)
        {
            if (Attribute != null)
            {
                Authorize(typeof(TQuery).Name);
            }

            return await decoratedHandler.HandleAsync(query, cancellationtoken);
        }
    }
}
