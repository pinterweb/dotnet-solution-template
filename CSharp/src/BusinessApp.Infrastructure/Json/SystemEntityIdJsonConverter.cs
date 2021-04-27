using BusinessApp.Kernel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BusinessApp.Infrastructure.Json
{
    public class SystemEntityIdJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeof(IEntityId).IsAssignableFrom(typeToConvert) ||
                typeof(IEntityId).IsAssignableFrom(Nullable.GetUnderlyingType(typeToConvert));

        public override JsonConverter CreateConverter(Type typeToConvert,
            JsonSerializerOptions options)
        {
            var converterType = typeof(SystemEntityIdJsonConverter<>)
                .MakeGenericType(typeToConvert);

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private class SystemEntityIdJsonConverter<TEntityId> : JsonConverter<TEntityId>
            where TEntityId : IEntityId
        {
            private static readonly string typeDisplayName = typeof(TEntityId).Name;

            public override TEntityId? Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null) return default;

                var converter = TypeDescriptor.GetConverter(typeToConvert);

                if (reader.TokenType == JsonTokenType.String
                    && converter.CanConvertFrom(typeof(string)))
                {
                    return (TEntityId)converter.ConvertFrom(reader.GetString());
                }
                else if (CanTryToConvertNumber(reader, converter, out var typeToConvertTo))
                {
                    var longConverter = TypeDescriptor.GetConverter(typeof(long));

                    return (TEntityId)converter.ConvertFrom(
                            longConverter.ConvertTo(reader.GetInt64(), typeToConvertTo));

                }

                throw new FormatException($"Cannot convert {typeDisplayName}");
            }

            public override void Write(Utf8JsonWriter writer, TEntityId value, JsonSerializerOptions options)
            {
                if (value is IEntityId id)
                {
                    var innerValue = Convert.ChangeType(id, id.GetTypeCode(), CultureInfo.InvariantCulture);

                    JsonSerializer.Serialize(writer, innerValue, options);
                }
                else
                {
                    throw new NotSupportedException("Cannot write the json value because the " +
                        "source value is not an IEntityId");
                }
            }

            // all json values are long, so we need to convert to actual type
            private static bool CanTryToConvertNumber(Utf8JsonReader reader, TypeConverter converter,
                out Type? typeToConvertTo)
            {
                typeToConvertTo = null;

                if (!reader.TryGetInt64(out var _)) return false;

                if (converter.CanConvertFrom(typeof(int))) typeToConvertTo = typeof(int);

                if (converter.CanConvertFrom(typeof(short))) typeToConvertTo = typeof(short);

                return true;
            }
        }
    }
}
