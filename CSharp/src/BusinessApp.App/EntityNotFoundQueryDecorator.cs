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
                throw new EntityNotFoundException(typeof(TResult).Name);
            }

            return result;
        }
    }
}
