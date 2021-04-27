using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi
{
    public static class HttpResponseExtensions
    {
        public static bool IsSuccess(this HttpResponse response)
            => response.StatusCode is >= 200 and <= 299;
    }
}
