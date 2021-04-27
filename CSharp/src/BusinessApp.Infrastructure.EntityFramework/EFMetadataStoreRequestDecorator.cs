using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using System.Security.Principal;

namespace BusinessApp.Infrastructure.EntityFramework
{
    /// <summary>
    /// Persist the requests
    /// </summary>
    public class EFMetadataStoreRequestDecorator<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : class
    {
        private readonly BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly IRequestHandler<TRequest, TResponse> inner;
        private readonly IEntityIdFactory<MetadataId> idFactory;

        public EFMetadataStoreRequestDecorator(IRequestHandler<TRequest, TResponse> inner,
            IPrincipal user, BusinessAppDbContext db, IEntityIdFactory<MetadataId> idFactory)
        {
            this.user = user.NotNull().Expect(nameof(user));
            this.inner = inner.NotNull().Expect(nameof(inner));
            this.db = db.NotNull().Expect(nameof(inner));
            this.idFactory = idFactory.NotNull().Expect(nameof(idFactory));
        }

        public Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            var eventId = idFactory.Create();
            var metadata = new Metadata<TRequest>(eventId,
                user.Identity?.Name ?? AnonymousUser.Name,
                MetadataType.Request,
                request);

            _ = db.Add(metadata);

            return inner.HandleAsync(request, cancelToken);
        }
    }
}
