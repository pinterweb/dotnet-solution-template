﻿namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class AuthorizationRequestDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    {
        private readonly IRequestHandler<TRequest, TResult> inner;
        private readonly IAuthorizer<TRequest> authorizer;

        public AuthorizationRequestDecorator(
            IRequestHandler<TRequest, TResult> inner,
            IAuthorizer<TRequest> authorizer
        )
        {
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
            this.authorizer = Guard.Against.Null(authorizer).Expect(nameof(authorizer));
        }

        public Task<Result<TResult, IFormattable>> HandleAsync(TRequest query, CancellationToken cancellationToken)
        {
            authorizer.AuthorizeObject(query);

            return inner.HandleAsync(query, cancellationToken);
        }
    }
}