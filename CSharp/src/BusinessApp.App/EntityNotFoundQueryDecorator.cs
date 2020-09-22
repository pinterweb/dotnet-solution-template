namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Throws a business exception if no entity is found from the query
    /// </summary>
    public class EntityNotFoundQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> decorated;

        public EntityNotFoundQueryDecorator(IQueryHandler<TQuery, TResult> decorated)
        {
            this.decorated = Guard.Against.Null(decorated).Expect(nameof(decorated));
        }

        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            var result = await decorated.HandleAsync(query, cancellationToken);

            if (result == null)
            {
                throw new EntityNotFoundException("The data you tried to search for was not " +
                    "found based on your search critiera. Try to change your criteria and search again. " +
                    "If the data is still found, it may have been deleted.");
            }

            return result;
        }
    }
}
