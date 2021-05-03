using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Extracts metadata out of an <see cref="HttpRequest" />
    /// </summary>
    public interface IHttpRequestAnalyzer
    {
        Task<HttpRequestPayloadType> GetBodyTypeAsync(HttpRequest request);
    }
}
