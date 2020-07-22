namespace BusinessApp.App.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.Value == null) return null;

            var converter = TypeDescriptor.GetConverter(objectType);

            if (converter.CanConvertFrom(typeof(T)))
            {
                return converter.ConvertFrom(
                    Convert.ChangeType(reader.Value, typeof(T))
                );
            }

            return serializer.Deserialize(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                T id = value as EntityId<T>;
                JToken finalValue;

                if (typeof(T) == typeof(long))
                {
                    finalValue = JToken.FromObject(id.ToString());
                }
                else
                {
                    finalValue = JToken.FromObject(id);
                }

                finalValue.WriteTo(writer);
            }
        }

    }
}
