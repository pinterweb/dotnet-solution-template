namespace BusinessApp.WebApi
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public interface IHttpRequestAnalyzer
    {
        Task<HttpRequestPayloadType> GetBodyTypeAsync(HttpRequest request);
    }
}
