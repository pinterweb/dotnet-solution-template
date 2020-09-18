namespace BusinessApp.WebApi
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static async Task<bool> HasBody(this HttpRequest request)
        {
            var firstByte = await request.BodyReader.ReadAsync();

            return !firstByte.Buffer.IsEmpty;
        }
    }
}
