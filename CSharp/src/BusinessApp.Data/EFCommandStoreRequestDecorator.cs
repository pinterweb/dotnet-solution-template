namespace BusinessApp.Data
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using BusinessApp.App;
    using System.Security.Principal;

    /// <summary>
    /// Persist the requests
    /// </summary>
    public class EFCommandStoreRequestDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    {
        private readonly BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly IRequestHandler<TRequest, TResponse> inner;

        public EFCommandStoreRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IPrincipal user, BusinessAppDbContext db)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.db = db.NotNull().Expect(nameof(inner));
        }

        public Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            db.Add(new RequestMetadata<TRequest>(request, user.Identity.Name));

            return inner.HandleAsync(request, cancelToken);
        }
    }
}
