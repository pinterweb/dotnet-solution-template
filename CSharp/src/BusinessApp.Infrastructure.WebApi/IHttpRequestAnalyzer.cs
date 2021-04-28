using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.Infrastructure.WebApi
{
    public interface IHttpRequestAnalyzer
    {
        Task<HttpRequestPayloadType> GetBodyTypeAsync(HttpRequest request);
    }
}
