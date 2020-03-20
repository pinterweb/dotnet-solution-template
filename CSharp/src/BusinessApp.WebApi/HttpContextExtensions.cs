namespace BusinessApp.WebApi
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using BusinessApp.App;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Deserializes the uri or body depending on the request method
        /// </summary>
        public static T DeserializeInto<T>(this HttpContext context,
            ISerializer serializer
        )
            where T : class, new()
        {
            if (context.Request.Method.Equals("get", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Method.Equals("delete", StringComparison.OrdinalIgnoreCase)
                )
            {
                // TODO might not be able to do Sync. Deserialization see ShelfLife 3.1 commit
                using (var stream = DeserializeUri(context, serializer))
                {
                    return serializer.Deserialize<T>(stream) ?? new T();
                }
            }
            else if (context.Request.ContentLength == null || context.Request.ContentLength > 0)
            {
                return serializer.Deserialize<T>(context.Request.Body) ?? new T();
            }

            return new T();
        }

        /// <summary>
        /// Helper class to create the <see cref="ResponseProblemBody.Type />
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        public static Uri CreateProblemUri(this HttpContext context, string errorType)
            => new Uri($"{context.Request.Scheme}://{context.Request.Host}/docs/errors/{errorType}");

        private static Stream DeserializeUri(HttpContext context, ISerializer serializer)
        {
            var collection = HttpUtility.ParseQueryString(context.Request.QueryString.Value);

            foreach (var r in context.GetRouteData().Values)
            {
                collection.Add(r.Key, r.Value.ToString());
            }

            var stream = new MemoryStream();

            if (collection.HasKeys())
            {
                serializer.Serialize(
                    stream,
                    collection.AllKeys.ToDictionary(key => key, key => collection[key])
                );

                stream.Position = 0;
            }

            return stream;
        }
    }
}
