namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Wraps a request for a single response object in an IEnumerable request handler
    /// when batch requests are supported
    /// </summary>
    /// <remarks>
    /// This help reduce the number of handlers/decorators needed when all we care about
    /// is returning one resource
    /// </remarks>
    public class SingleQueryRequestAdapter<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse>
        where TRequest : notnull, IQuery
    {
        private static Exception MoreThanOneResultErr =
            new BusinessAppAppException("Your query expected to return one result, but " +
                "for some reason more than one result was returned. Please try the " +
                "request again or contact support");

        private readonly IRequestHandler<TRequest, IEnumerable<TResponse>> handler;

        public SingleQueryRequestAdapter(IRequestHandler<TRequest, IEnumerable<TResponse>> handler)
        {
            this.handler = handler.NotNull().Expect(nameof(handler));
        }

        public async Task<Result<TResponse, Exception>> HandleAsync(TRequest request,
            CancellationToken cancelToken)
        {
            return await handler.HandleAsync(request, cancelToken)
                .AndThenAsync(r => r.Count() == 1
                    ? Result.Ok(r)
                    : Result.Error<IEnumerable<TResponse>>(MoreThanOneResultErr))
                .MapAsync(r => r.Single());
        }
    }
}
