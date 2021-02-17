namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using Microsoft.AspNetCore.Http;

    public static partial class HttpContextExtensions
    {
        /// <summary>
        /// Deserializes the uri or body depending on the request method
        /// </summary>
        public static Task<T> DeserializeIntoAsync<T>(this HttpContext context,
            ISerializer serializer,
            CancellationToken cancelToken)
        {
            if (context.Request.MayHaveContent())
            {
                var model = serializer.Deserialize<T>(context.Request.Body);

                if (context.Request.RouteValues.Count > 0)
                {
                    GenericSerializationHelpers<T>.SetProperties(model, context.Request.RouteValues);
                }

                return Task.FromResult(model);
            }

            using (var stream = GenericSerializationHelpers<T>.SerializeRouteAndQueryValues(context, serializer))
            {
                return Task.FromResult(serializer.Deserialize<T>(stream));
            }
        }
    }
}
