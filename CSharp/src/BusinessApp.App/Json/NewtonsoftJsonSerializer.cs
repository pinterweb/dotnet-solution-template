namespace BusinessApp.App.Json
{
    using System.IO;
    using System.Text;
    using BusinessApp.App;
    using Newtonsoft.Json;

    /// <summary>
    /// A json serializer based on Newtonsoft
    /// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializer serializer;

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings)
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
