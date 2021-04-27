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

        public T? Deserialize<T>(byte[] data) => data.Length > 0 ?
            JsonSerializer.Deserialize<T>(data.AsSpan(), options) :
            default;

        public byte[] Serialize<T>(T graph)
            => JsonSerializer.SerializeToUtf8Bytes(graph, options);
    }
}
