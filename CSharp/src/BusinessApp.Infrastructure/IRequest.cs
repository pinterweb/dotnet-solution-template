namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Defines a query message
    /// </summary>
    /// <typeparam name="TResult">
    /// The result set type returned from the query
    /// </typeparam>
    public interface IRequest<out TResponse>
    {
    }
}
