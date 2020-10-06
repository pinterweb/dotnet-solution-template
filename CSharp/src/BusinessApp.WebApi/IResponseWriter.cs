namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public interface IResponseWriter
    {
        Task WriteResponseAsync<T, E>(HttpContext context, Result<T, E> result) where E : IFormattable;
        Task WriteResponseAsync(HttpContext context);
    }
}
