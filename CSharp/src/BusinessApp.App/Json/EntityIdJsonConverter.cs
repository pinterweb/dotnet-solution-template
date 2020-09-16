namespace BusinessApp.App.Json
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
    using BusinessApp.Domain;

    /// <summary>
    /// Converts to and from the complex <see cref="EntityId{T}"/> and primitive {T} Id type
    /// </summary>
    public class EntityIdJsonConverter<T> : JsonConverter
        where T : IComparable
    {
        public override bool CanConvert(Type objectType)
            => typeof(EntityId<T>).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.Value == null) return null;

            var converter = TypeDescriptor.GetConverter(objectType);

            if (converter.CanConvertFrom(reader.ValueType))
            {
                try
                {
                    return converter.ConvertFrom(reader.Value);
                }
                catch
                {
                    throw new FormatException($"Cannot read value for '{reader.Path}' because the " +
                        $"type is incorrect. Expected a '{typeof(T).Name}'.");
                }
            }

            throw new FormatException($"Cannot read value for '{reader.Path}' because the " +
                $"type is incorrect. Expected a '{typeof(T).Name}', but read a '{reader.ValueType.Name}'.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var converter = TypeDescriptor.GetConverter(value);

            if (converter.CanConvertTo(typeof(T)))
            {
                var innerValue = converter.ConvertTo(value, typeof(T));

                serializer.Serialize(writer, innerValue);
            }
            else
            {
                throw new NotSupportedException("Cannot write the EntityId to a json value. " +
                    $"Cannot convert from '{value.GetType().Name}' to '{typeof(T).Name}'.");
            }
        }
    }
}
