using System;
using System.Text.Json;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.Json
{
    /// <summary>
    /// System implementation for json serializer
    /// </summary>
    public class SystemJsonSerializerAdapter : ISerializer
    {
        private readonly JsonSerializerOptions options;

        public SystemJsonSerializerAdapter(JsonSerializerOptions options)
            => this.options = options.NotNull().Expect(nameof(options));

        public T? Deserialize<T>(byte[] data)
        {
            try
            {
                return data.Length > 0 ?
                    JsonSerializer.Deserialize<T>(data.AsSpan(), options) :
                    default;
            }
            catch (JsonException e)
            {
                throw new JsonDeserializationException("An error occurred while reading your JSON data", e);
            }
        }

        public byte[] Serialize<T>(T graph)
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(graph, options);
            }
            catch (JsonException e)
            {
                throw new JsonSerializationException("An error occurred while converting your object to JSON", e);
            }
        }
    }
}
