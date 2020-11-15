namespace BusinessApp.App.Json
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
    using BusinessApp.Domain;

    /// <summary>
    /// Converts to and from the complex <see cref="EntityId{T}"/> and primitive {T} Id type
    /// </summary>
    public class EntityIdJsonConverter : JsonConverter
    {
        private static TypeCode[] NumberTypeCodes = new[]
        {
            TypeCode.Int16,
            TypeCode.Int32,
            TypeCode.Int64,
        };

        private static readonly string ErrTemplate = "Cannot read value for '{0}' because the type is incorrect";

        public override bool CanConvert(Type objectType) =>
        (
            typeof(IEntityId).IsAssignableFrom(objectType) ||
            typeof(IEntityId).IsAssignableFrom(Nullable.GetUnderlyingType(objectType))
        )
            && objectType.IsValueType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.Value == null) return null;

            var converter = TypeDescriptor.GetConverter(objectType);

            try
            {
                if (CanTryToConvertNumber(reader, converter))
                {
                    var defaultValue = (IEntityId)Activator.CreateInstance(
                        Nullable.GetUnderlyingType(objectType) ?? objectType);

                    return converter.ConvertFrom(Convert.ChangeType(reader.Value, defaultValue.GetTypeCode()));
                }
                if (converter.CanConvertTo(reader.ValueType) || CanTryToConvertNumber(reader, converter))
                {
                        return converter.ConvertFrom(reader.Value);
                }
            }
            catch(Exception e)
            {
                throw new BusinessAppAppException(string.Format(ErrTemplate, reader.Path), e);
            }

            throw new BusinessAppAppException(string.Format(ErrTemplate, reader.Path));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IEntityId id)
            {
                var innerValue = Convert.ChangeType(id, id.GetTypeCode());

                serializer.Serialize(writer, innerValue);
            }
            else
            {
                throw new NotSupportedException("Cannot write the json value because the " +
                    "source value is not an IEntityId");
            }
        }

        // all json values are long, so we need to convert to actual type
        private static bool CanTryToConvertNumber(JsonReader reader, TypeConverter converter)
        {
            return reader.ValueType == typeof(long) &&
            (
                converter.CanConvertFrom(typeof(int)) ||
                converter.CanConvertFrom(typeof(short))
            );
        }
    }
}
