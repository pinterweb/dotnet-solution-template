namespace BusinessApp.WebApi
{
    using BusinessApp.App;
    using System.Collections.Generic;

    // [Proxy(typeof(ValidationRequestDecorator<,>))]
    internal sealed class MacroScopeWrappingHandler<TRequest, TResponse> :
         BatchScopeWrappingHandler<BatchRequestDelegator<TRequest, TResponse>, IEnumerable<TRequest>, IEnumerable<TResponse>>
    {
        public MacroScopeWrappingHandler(BatchRequestDelegator<TRequest, TResponse> inner)
            : base(inner)
        { }
    }
}
