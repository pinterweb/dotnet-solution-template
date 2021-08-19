using Newtonsoft.Json;
using BusinessApp.Kernel;
using System.IO;
using System.Text;

namespace BusinessApp.Infrastructure.Json
{
    /// <summary>
    /// A json serializer based on Newtonsoft
    /// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;
        private readonly JsonSerializer serializer;

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings)
        {
            this.settings = settings.NotNull().Expect(nameof(settings));
            serializer = JsonSerializer.Create(settings);
        }

        public T? Deserialize<T>(byte[] data)
        {
            using var serializationStream = new MemoryStream(data);
            using var sr = new StreamReader(serializationStream);
            using var jr = new JsonTextReader(sr);

            try
            {
                return serializer.Deserialize<T>(jr);

            }
            catch (JsonReaderException e)
            {
                throw new JsonDeserializationException("An error occurred while reading your JSON data", e);
            }
        }

        public byte[] Serialize<T>(T graph)
        {
            try
            {
                var jsonStr = JsonConvert.SerializeObject(graph, settings);

                return Encoding.UTF8.GetBytes(jsonStr);
            }
            catch (JsonException e)
            {
                throw new JsonSerializationException("An error occurred while converting your object to JSON", e);
            }
        }
    }
}
