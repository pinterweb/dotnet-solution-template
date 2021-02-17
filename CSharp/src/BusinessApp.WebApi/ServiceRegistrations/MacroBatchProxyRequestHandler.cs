namespace BusinessApp.WebApi
{
    using BusinessApp.App;
    using System.Collections.Generic;

    /// <summary>
    /// Proxies the request to the <see cref="BatchRequestAdapter{TRequest, TResponse}"/>
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <remarks>Only purpose is signal in the pipeline that a macro caused this</remarks>
    internal sealed class MacroBatchProxyRequestHandler<TRequest, TResponse> :
         BatchProxyRequestHandler<BatchRequestAdapter<TRequest, TResponse>, IEnumerable<TRequest>, IEnumerable<TResponse>>
    {
        public MacroBatchProxyRequestHandler(BatchRequestAdapter<TRequest, TResponse> inner)
            : base(inner)
        { }
    }
}
