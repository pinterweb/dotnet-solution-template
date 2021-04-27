using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace BusinessApp.Infrastructure.Json
{
    /// <summary>
    /// Converts any long type to string since Javascript cannot handle longs
    /// </summary>
    public class NewtonsoftLongToStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(long);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
            => reader.TokenType == JsonToken.Null
                ? null
                : Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var finalValue = JToken.FromObject(value.ToString()!);
                finalValue.WriteTo(writer);
            }
        }
    }
}
