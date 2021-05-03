using Microsoft.AspNetCore.Http;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Extensions for <see cref="HttpResponse" />
    /// </summary>
    public static class HttpResponseExtensions
    {
        public static bool IsSuccess(this HttpResponse response)
            => response.StatusCode is >= 200 and <= 299;
    }
}
