namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;

    public static class HttpResponseExtensions
    {
        public static bool IsSuccess(this HttpResponse response)
        {
            return response.StatusCode >= 200 && response.StatusCode <= 299;
        }
    }
}
