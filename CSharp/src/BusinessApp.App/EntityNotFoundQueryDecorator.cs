namespace BusinessApp.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Throws a business exception if no entity is found from the query
    /// </summary>
    public class EntityNotFoundQueryDecorator<TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery
    {
        private readonly IRequestHandler<TQuery, TResult> decorated;

        public EntityNotFoundQueryDecorator(IRequestHandler<TQuery, TResult> decorated)
        {
            this.decorated = decorated.NotNull().Expect(nameof(decorated));
        }

        public async Task<Result<TResult, IFormattable>> HandleAsync(
            TQuery query, CancellationToken cancellationToken)
        {
            var result = await decorated.HandleAsync(query, cancellationToken);

            return result.AndThen(val =>
            {
                if (val == null)
                {
                    return Result<TResult, IFormattable>.Error(
                        new EntityNotFoundException("The data you tried to search for was not " +
                            "found based on your search critiera. Try to change your criteria " +
                            "and search again. If the data is still found, it may have been deleted.")
                    );
                }

                return result;
            });
        }
    }
}
