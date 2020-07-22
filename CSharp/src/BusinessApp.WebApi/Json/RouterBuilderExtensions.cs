namespace BusinessApp.WebApi.Json
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Newtonsoft.Json;

    public static class RouteBuilderExtensions
    {
        /// <summary>
        /// Delegates a request based on if the payload in the
        /// body is an array or single object
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="template"></param>
        /// <param name="one"></param>
        /// <param name="many"></param>
        public static void MapPostOneOrMany(this IRouteBuilder builder,
                string template,
                RequestDelegate one,
                RequestDelegate many)
        {
            builder.MapPost(template, ctx =>
            {
                if (ctx.Request.BodyType().GetAwaiter().GetResult() != JsonToken.StartArray)
                {
                    return one(ctx);
                }
                else
                {
                    return many(ctx);
                }
            });
        }

        public static async Task<JsonToken> BodyType(this HttpRequest request)
        {
            request.EnableBuffering();

            // Leave the body open so the next middleware can read it.
            using (
                var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true
                )
            )
            using (var jsonReader = new JsonTextReader(reader))
            {
                await jsonReader.ReadAsync();
                request.Body.Position = 0;

                return jsonReader.TokenType;
            }
        }
    }
}
