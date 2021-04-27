using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using System;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Interface to handle an HTTP request
    /// </summary>
    public interface IHttpRequestHandler
    {
        /// <summary>
        /// Handles an HTTP request via the <see cref="HttpContext" /> and converts it
        /// to a <see cref="HandlerContext{TIn, TOut}" />
        /// </summary>
        /// <typeparam name="TIn">The input type</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        Task HandleAsync<TIn, TOut>(HttpContext context) where TIn : notnull;
    }

    /// <summary>
    /// Interface to handle an HTTP request and convert it to a <see cref="HandlerContext{T, R}" />
    /// </summary>
    /// <typeparam name="TIn">The request type</typeparam>
    /// <typeparam name="TOut">The response type</typeparam>
    public interface IHttpRequestHandler<TIn, TOut>
        where TIn : notnull
    {
        Task<Result<HandlerContext<TIn, TOut>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken);
    }
}
