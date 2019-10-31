namespace BusinessApp.WebApi.Json
{
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Default json serializer
    /// </summary>
    public class JsonFormatter : ISerializer
    {
        private readonly JsonSerializer serializer;

        public JsonFormatter(JsonSerializerSettings settings)
        {
            serializer = JsonSerializer.Create(settings);
        }

        public T Deserialize<T>(Stream serializationStream)
        {
            using (var sr = new StreamReader(serializationStream))
            using (var jr = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jr);
            }
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            using (
                var writer = new StreamWriter(serializationStream,
                    new UTF8Encoding(false),
                    1024,
                    true
                )
            )
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                serializer.Serialize(jsonWriter, graph);
                jsonWriter.Flush();
            }
        }
    }
}
