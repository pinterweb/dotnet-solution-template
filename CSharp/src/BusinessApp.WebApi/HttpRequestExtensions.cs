namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static bool HasBody(this HttpRequest request)
        {
            request.EnableBuffering();
            var hasBody = request.Body.ReadByte() != -1;
            request.Body.Position = 0;

            return hasBody;
        }
    }
}
