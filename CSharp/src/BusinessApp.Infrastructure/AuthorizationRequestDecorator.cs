using System;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    public class AuthorizationRequestDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : notnull
    {
        private readonly IRequestHandler<TRequest, TResult> inner;
        private readonly IAuthorizer<TRequest> authorizer;
        private readonly IPrincipal user;
        private readonly ILogger logger;

        public AuthorizationRequestDecorator(IRequestHandler<TRequest, TResult> inner,
            IAuthorizer<TRequest> authorizer, IPrincipal user, ILogger logger)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.authorizer = authorizer.NotNull().Expect(nameof(authorizer));
            this.user = user.NotNull().Expect(nameof(user));
            this.logger = logger.NotNull().Expect(nameof(logger));
        }

        public Task<Result<TResult, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            if (!authorizer.AuthorizeObject(request))
            {
                LogUnauthAcess(request);
                var ex = BuildAuthException();

                return Task.FromResult(Result.Error<TResult>(ex));
            }

            return inner.HandleAsync(request, cancelToken);
        }

        private void LogUnauthAcess(TRequest request)
        {
            var userName = user.Identity?.Name ?? AnonymousUser.Name;
            var message = $"'{userName}' is not authorized to execute {request.GetType().FullName}";

            logger.Log(new LogEntry(LogSeverity.Info, message)
            {
                Data = request
            });
        }

        private Exception BuildAuthException()
        {
            var message = $"You are not authorized to execute this request";

            return new SecurityException(message);
        }
    }
}
