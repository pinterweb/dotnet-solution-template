namespace BusinessApp.WebApi
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using Microsoft.AspNetCore.Http;

    public static partial class HttpContextExtensions
    {
        /// <summary>
        /// Deserializes the uri or body depending on the request method
        /// </summary>
        public static async Task<T> DeserializeIntoAsync<T>(this HttpContext context,
            ISerializer serializer,
            CancellationToken cancellationToken)
        {
            if (!context.Request.Method.Equals("get", StringComparison.OrdinalIgnoreCase) &&
                !context.Request.Method.Equals("delete", StringComparison.OrdinalIgnoreCase) &&
                (context.Request.ContentLength == null || context.Request.ContentLength > 0)
            )
            {
                var body = await context.Request.BodyReader.ReadAsync(cancellationToken);

                using var ms = new MemoryStream(body.Buffer.ToArray());
                var model = serializer.Deserialize<T>(ms);


                if (context.Request.RouteValues.Count > 0)
                {
                    GenericSerializationHelpers<T>.SetProperties(model, context.Request.RouteValues);
                }
            }

            using (var stream = GenericSerializationHelpers<T>.DeserializeUri(context, serializer))
            {
                return serializer.Deserialize<T>(stream);
            }
        }
    }
}
