namespace BusinessApp.App
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles query data in the <typeparam name="TQuery">TQuery</typeparam>
    /// </summary>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
