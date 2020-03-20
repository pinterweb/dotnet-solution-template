namespace BusinessApp.WebApi.Json
{
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using BusinessApp.App;

    public static class JsonSerializationHelpers
    {
        public static HttpContent CreateJsonContent(ISerializer serializer, object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                using (var ms = new MemoryStream())
                {
                    serializer.Serialize(ms, content);
                    ms.Seek(0, SeekOrigin.Begin);
                    httpContent = new StreamContent(ms);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
            }

            return httpContent;
        }
    }
}
