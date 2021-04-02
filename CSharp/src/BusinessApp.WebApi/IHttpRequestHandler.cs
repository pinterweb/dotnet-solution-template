namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using System;

    /// <summary>
    /// Interface to handle an HTTP request
    /// </summary>
    public interface IHttpRequestHandler
    {
        /// <summary>
        /// Handles an HTTP request via the <see cref="HttpContext" /> and converts it
        /// to a <see cref="HandlerContext{T, R}" />
        /// </summary>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="R">The response type</typeparam>
        Task HandleAsync<T, R>(HttpContext context) where T : notnull;
    }

    /// <summary>
    /// Interface to handle an HTTP request and convert it to a <see cref="HandlerContext{T, R}" />
    /// </summary>
    /// <typeparam name="T">The request type</typeparam>
    /// <typeparam name="R">The response type</typeparam>
    public interface IHttpRequestHandler<T, R>
        where T : notnull
    {
        Task<Result<HandlerContext<T, R>, Exception>> HandleAsync(
            HttpContext context, CancellationToken cancelToken);
    }
}
