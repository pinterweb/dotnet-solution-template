using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BusinessApp.Kernel;
using System.IO;

namespace BusinessApp.Infrastructure.Json
{
#pragma warning disable IDE0065
    using Newtonsoft.Json.Serialization;
#pragma warning restore IDE0065

    /// <summary>
    /// A json serializer based on Newtonsoft
    /// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings;
        private readonly JsonSerializer serializer;
        private List<MemberValidationException> errors;

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings)
        {
            this.settings = settings.NotNull().Expect(nameof(settings));
            serializer = JsonSerializer.Create(settings);
            errors = new List<MemberValidationException>();
        }

        public T? Deserialize<T>(byte[] data)
        {
            errors = new List<MemberValidationException>();
            serializer.Error += OnError;

            using var serializationStream = new MemoryStream(data);
            using var sr = new StreamReader(serializationStream);
            using var jr = new JsonTextReader(sr);

            try
            {
                var model = serializer.Deserialize<T>(jr);

                return !errors.Any()
                   ? model
                   : throw new ModelValidationException("There is bad data", errors);
            }
            finally
            {
                serializer.Error -= OnError;
            }
        }

        public byte[] Serialize<T>(T graph)
        {
            var str = JsonConvert.SerializeObject(graph, settings);

            return Encoding.UTF8.GetBytes(str);
        }

        private void OnError(object? sender, ErrorEventArgs e)
        {
            var args = e;
            var memberName = args.ErrorContext.Member?.ToString();

            if (!string.IsNullOrWhiteSpace(memberName))
            {
                var error = new MemberValidationException(memberName,
                    new[] { args.ErrorContext.Error.Message });

                errors.Add(error);

                args.ErrorContext.Handled = true;
            }
        }
    }
}
