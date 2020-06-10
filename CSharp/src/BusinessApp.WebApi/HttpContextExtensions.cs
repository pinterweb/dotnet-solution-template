namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.App;
    using Microsoft.AspNetCore.Http;

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Deserializes the uri or body depending on the request method
        /// </summary>
        public static T DeserializeInto<T>(this HttpContext context,
            ISerializer serializer)
        {
            if (context.Request.Method.Equals("get", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Method.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                // TODO might not be able to do Sync. Deserialization see ShelfLife 3.1 commit
                using (var stream = GenericSerializationHelpers<T>.DeserializeUri(context, serializer))
                {
                    return serializer.Deserialize<T>(stream);
                }
            }
            else if (context.Request.ContentLength == null || context.Request.ContentLength > 0)
            {
                return serializer.Deserialize<T>(context.Request.Body);
            }

            return default(T);
        }

        /// <summary>
        /// Helper class to create the <see cref="ResponseProblemBody.Type"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        public static Uri CreateProblemUri(this HttpContext context, string errorType)
            => new Uri($"{context.Request.Scheme}://{context.Request.Host}/docs/errors/{errorType}");
    }
}
