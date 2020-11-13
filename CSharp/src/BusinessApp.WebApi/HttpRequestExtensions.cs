namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;
    using System.Linq;

    public static class HttpRequestExtensions
    {
        private static readonly string[] BodyMethods = new[]
        {
            HttpMethods.Patch,
            HttpMethods.Post,
            HttpMethods.Put
        };

        public static bool MayHaveContent(this HttpRequest request)
        {
            return BodyMethods.Contains(request.Method)
                && (request.ContentLength == null || request.ContentLength > 0);
        }
    }
}
