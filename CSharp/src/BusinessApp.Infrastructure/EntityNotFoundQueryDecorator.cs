using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Throws a business exception if no entity is found from the query
    /// </summary>
    public class EntityNotFoundQueryDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : notnull, IQuery
    {
        private readonly IRequestHandler<TRequest, TResponse> decorated;

        public EntityNotFoundQueryDecorator(IRequestHandler<TRequest, TResponse> decorated)
            => this.decorated = decorated.NotNull().Expect(nameof(decorated));

        public async Task<Result<TResponse, Exception>> HandleAsync(
            TRequest request, CancellationToken cancelToken)
        {
            var result = await decorated.HandleAsync(request, cancelToken);

            return result.AndThen(val => val == null
                ? Result.Error<TResponse>(new EntityNotFoundException(
                        "The data you tried to search for was not " +
                        "found based on your search critiera. Try to change your criteria " +
                        "and search again. If the data is still not found, it may have been deleted."))
                : result
            );
        }
    }
}
