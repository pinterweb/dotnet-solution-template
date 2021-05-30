using System.Diagnostics;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Class returned from <see cref="IHttpRequestHandler{TIn, TOut}" /> to
    /// simplify all data returned from handlers
    /// </summary>
    public class HandlerContext<TRequest, TResponse>
    {
        public HandlerContext(TRequest request, TResponse response)
        {
            Request = request;
            Response = response;
        }

        public TRequest Request { get; }
        public TResponse Response { get; }
    }

    [DebuggerStepThrough]
    public class HandlerContext
    {
        public static HandlerContext<TRequest, TResponse> Create<TRequest, TResponse>(TRequest request, TResponse response)
            => new(request, response);
    }
}
