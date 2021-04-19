using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BusinessApp.App.Json
{
    /// <summary>
    /// Converts any long type to string since Javascript cannot handle longs
    /// </summary>
    public class NewtonsoftLongToStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(long);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            return Convert.ToInt64(reader.Value);
        }

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
