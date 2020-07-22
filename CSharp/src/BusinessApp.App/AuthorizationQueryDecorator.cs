namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Authorizes a user based on the <typeparam name="TQuery">TQuery</typeparam>
    /// </summary>
    public class AuthorizationQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> decoratedHandler;
        private readonly IAuthorizer<TQuery> authorizer;

        public AuthorizationQueryDecorator(
            IQueryHandler<TQuery, TResult> decoratedHandler,
            IAuthorizer<TQuery> authorizer
        )
        {
            this.decoratedHandler = GuardAgainst.Null(decoratedHandler, nameof(decoratedHandler));
            this.authorizer = GuardAgainst.Null(authorizer, nameof(authorizer));
        }

        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            authorizer.AuthorizeObject(query);

            return decoratedHandler.HandleAsync(query, cancellationToken);
        }
    }
}
