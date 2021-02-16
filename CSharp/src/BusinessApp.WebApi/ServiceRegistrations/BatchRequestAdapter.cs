namespace BusinessApp.WebApi
{
    using BusinessApp.App;

    /// <summary>
    /// Runs the same logic for each request
    /// </summary>
    // TODO Adapter
    public class BatchRequestDelegator<TConsumer, TRequest, TResponse> : BatchRequestDelegator<TRequest, TResponse>
    {
        public BatchRequestDelegator(IRequestHandler<TRequest, TResponse> inner)
            : base(inner)
        {
        }
    }
}
