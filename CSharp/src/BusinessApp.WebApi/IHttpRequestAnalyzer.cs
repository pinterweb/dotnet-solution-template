using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi
{
    public interface IHttpRequestAnalyzer
    {
        Task<HttpRequestPayloadType> GetBodyTypeAsync(HttpRequest request);
    }
}
