using System.IO;

namespace BusinessApp.App.Json
{
    using System.Text;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// A json serializer based on Newtonsoft
    /// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly ILogger logger;
        private readonly JsonSerializerSettings settings;
        private readonly JsonSerializer serializer;
        private List<MemberValidationException> errors;

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings, ILogger logger)
        {
            this.logger = logger.NotNull().Expect(nameof(logger));
            this.settings = settings.NotNull().Expect(nameof(logger));
            serializer = JsonSerializer.Create(settings);
        }

        public T Deserialize<T>(byte[] data)
        {
            using var serializationStream = new MemoryStream(data);
            errors = new List<MemberValidationException>();
            serializer.Error += OnError;
            using (var sr = new StreamReader(serializationStream))
            using (var jr = new JsonTextReader(sr))
            {
                try
                {
                    var model =  serializer.Deserialize<T>(jr);

                    if (!errors.Any()) return model;

                    throw new ModelValidationException("There is bad data", errors);
                }
                finally
                {
                    serializer.Error -= OnError;
                }
            }
        }

        public byte[] Serialize<T>(T graph)
        {
            var str = JsonConvert.SerializeObject(graph, settings);

            return Encoding.UTF8.GetBytes(str);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var error = new MemberValidationException(
                e.ErrorContext.Member?.ToString(),
                new[] { e.ErrorContext.Error.Message });

            errors.Add(error);

            logger.Log(
                new LogEntry(
                    LogSeverity.Error,
                    "Deserialization failed",
                    e.ErrorContext.Error,
                    e.ErrorContext.OriginalObject)
            );

            e.ErrorContext.Handled = true;
        }
    }
}
