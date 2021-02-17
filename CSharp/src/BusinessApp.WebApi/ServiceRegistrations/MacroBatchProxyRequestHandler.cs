namespace BusinessApp.WebApi
{
    using BusinessApp.App;
    using System.Collections.Generic;

    /// <summary>
    /// Proxies the request to the <see cref="BatchRequestDelegator{TRequest, TResponse}"/>
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <remarks>Only purpose is signal in the pipeline that a macro caused this</remarks>
    internal sealed class MacroBatchProxyRequestHandler<TRequest, TResponse> :
         BatchProxyRequestHandler<BatchRequestDelegator<TRequest, TResponse>, IEnumerable<TRequest>, IEnumerable<TResponse>>
    {
        public MacroBatchProxyRequestHandler(BatchRequestDelegator<TRequest, TResponse> inner)
            : base(inner)
        { }
    }
}
