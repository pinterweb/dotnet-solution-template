using System.Diagnostics;

namespace BusinessApp.WebApi
{
    public class HandlerContext<T, R>
    {
        public HandlerContext(T request, R response)
        {
            Request = request;
            Response = response;
        }

        public T Request { get; }
        public R Response { get; }
    }

    [DebuggerStepThrough]
    public class HandlerContext
    {
        public static HandlerContext<T, R> Create<T, R>(T request, R response)
        {
            return new HandlerContext<T, R>(request, response);
        }
    }
}
