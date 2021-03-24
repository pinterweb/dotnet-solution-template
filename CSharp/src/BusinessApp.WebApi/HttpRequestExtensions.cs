namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Http;
    using System.Linq;
    using System.Buffers;
    using System.IO.Pipelines;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;

    public static class HttpRequestExtensions
    {
        private static readonly string[] BodyMethods = new[]
        {
            HttpMethods.Patch,
            HttpMethods.Post,
            HttpMethods.Put
        };

        /// <summary>
        /// Checks the request body to determine if there may be content
        /// </summary>
        /// <remarks>Could give false positives</remarks>
        public static bool MayHaveContent(this HttpRequest request)
        {
            return request.IsCommand()
                && (request.ContentLength == null || request.ContentLength > 0);
        }

        /// <summary>
        /// Checks if the request is a command method
        /// </summary>
        public static bool IsCommand(this HttpRequest request)
        {
            return BodyMethods.Contains(request.Method);
        }

        /// <summary>
        /// Deserializes the uri or body depending on the request method
        /// </summary>
        public static async Task<T> DeserializeAsync<T>(this HttpRequest request,
            ISerializer serializer,
            CancellationToken cancelToken)
        {
            if (request.MayHaveContent())
            {
                var bodyReader = request.BodyReader;
                ReadResult readResult = await bodyReader.ReadAsync();
                T model;

                try
                {
                    while (!readResult.IsCompleted && !cancelToken.IsCancellationRequested)
                    {
                        // https://github.com/dotnet/AspNetCore.Docs/issues/17039#issuecomment-587517416
                        // By passing in readResult.Buffer.Start for consumed,
                        // you are saying that you haven't consumed any of the ReadResult.
                        // By passing in readResult.Buffer.End, you are saying you have
                        // examined everything, which tells the Pipe to not free any buffers
                        // and the next time ReadAsync returns, more data would be in the buffer
                        // We need all of segments when we serialize, if we tell pipereader
                        // we consumed all, the buffer we need for serialization will be empty
                        bodyReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
                        readResult = await bodyReader.ReadAsync();
                    }

                    // now all the body payload has been read into buffer
                    var buffer = readResult.Buffer;
                    model = serializer.Deserialize<T>(buffer.ToArray());

                    // Finally, reset the EXAMINED POSITION here
                    bodyReader.RewindTo(readResult.Buffer);
                }
                finally
                {
                    await bodyReader.CompleteAsync();
                }

                if (request.RouteValues.Count > 0)
                {
                    HttpRequestSerializationHelpers<T>.SetProperties(model, request.RouteValues);
                }

                return model;
            }

            return HttpRequestSerializationHelpers<T>.SerializeRouteAndQueryValues(request,
                serializer);
        }
    }
}
