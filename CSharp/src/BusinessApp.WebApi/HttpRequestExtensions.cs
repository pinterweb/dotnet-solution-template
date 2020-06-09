namespace BusinessApp.WebApi
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static async Task<bool> HasBody(this HttpRequest request)
        {
            request.EnableBuffering();

            var hasBody = await request.Body.ReadAsync(new byte[1], 0, 1) != -1;

            request.Body.Position = 0;

            return hasBody;
        }
    }
}
