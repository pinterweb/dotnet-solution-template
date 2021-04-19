using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi
{
    public static class HttpResponseExtensions
    {
        public static bool IsSuccess(this HttpResponse response)
        {
            return response.StatusCode >= 200 && response.StatusCode <= 299;
        }
    }
}
