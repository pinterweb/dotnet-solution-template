namespace BusinessApp.App.Json
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel;
    using BusinessApp.Domain;

    /// <summary>
    /// Converts to and from the complex <see cref="EntityId{T}"/> and primitive {T} Id type
    /// </summary>
    public class NewtonsoftEntityIdJsonConverter : JsonConverter
    {
        private static readonly string ErrTemplate = "Cannot read value for '{0}' because the type is incorrect";

        public override bool CanConvert(Type objectType) =>
            typeof(IEntityId).IsAssignableFrom(objectType) ||
            typeof(IEntityId).IsAssignableFrom(Nullable.GetUnderlyingType(objectType));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.Value == null) return null;

            var converter = TypeDescriptor.GetConverter(objectType);

            try
            {
                if (CanTryToConvertNumber(reader, converter, out Type typeToConvertTo))
                {
                    var longConverter = TypeDescriptor.GetConverter(typeof(long));

                    return converter.ConvertFrom(longConverter.ConvertTo(reader.Value, typeToConvertTo));
                }
                if (converter.CanConvertTo(reader.ValueType) || CanTryToConvertNumber(reader, converter, out Type _))
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
        private static bool CanTryToConvertNumber(JsonReader reader, TypeConverter converter, out Type typeToConvertTo)
        {
            typeToConvertTo = null;

            if (reader.ValueType != typeof(long)) return false;

            if (converter.CanConvertFrom(typeof(int))) typeToConvertTo = typeof(int);

            if (converter.CanConvertFrom(typeof(short))) typeToConvertTo = typeof(short);

            return true;
        }
    }
}
