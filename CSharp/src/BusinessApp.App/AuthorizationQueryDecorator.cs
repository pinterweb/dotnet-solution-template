namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class AuthorizationQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> inner;
        private readonly IAuthorizer<TQuery> authorizer;

        public AuthorizationQueryDecorator(
            IQueryHandler<TQuery, TResult> inner,
            IAuthorizer<TQuery> authorizer
        )
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
            this.authorizer = Guard.Against.Null(authorizer).Expect(nameof(authorizer));
        }

        public Task<Result<TResult, IFormattable>> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            authorizer.AuthorizeObject(query);

            return inner.HandleAsync(query, cancellationToken);
        }
    }
}
