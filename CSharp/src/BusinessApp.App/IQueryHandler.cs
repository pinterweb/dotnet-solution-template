namespace BusinessApp.App
{
    /// <summary>
    /// Handles query data in the <typeparam name="TQuery">TQuery</typeparam>
    /// </summary>
    /// <implementers>
    /// Logic in this pipeline should not modify data
    /// </implementers>
    public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery
    {
    }
}
