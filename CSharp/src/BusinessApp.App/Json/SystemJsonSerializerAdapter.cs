namespace BusinessApp.App.Json
{
    using System;
    using System.Text.Json;
    using BusinessApp.Domain;

    /// <summary>
    /// System implementation for json serializer
    /// </summary>
    public class SystemJsonSerializerAdapter : ISerializer
    {
        private readonly JsonSerializerOptions options;

        public SystemJsonSerializerAdapter(JsonSerializerOptions options)
        {
            this.options = options.NotNull().Expect(nameof(options));
        }

        public T Deserialize<T>(byte[] data)
        {
            return data.Length > 0 ?
                JsonSerializer.Deserialize<T>(data.AsSpan(), options) :
                default(T);
        }

        public byte[] Serialize<T>(T graph)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(graph, options);
        }
    }
}
