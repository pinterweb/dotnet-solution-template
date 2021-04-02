namespace BusinessApp.App
{
    /// <summary>
    /// Defines a query message
    /// </summary>
    /// <typeparam name="TResult">
    /// The result set type returned from the query
    /// </typeparam>
    [System.Obsolete]
    public interface IRequest<out TResponse>
    {
    }
}
