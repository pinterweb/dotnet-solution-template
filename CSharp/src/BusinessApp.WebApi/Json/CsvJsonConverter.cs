namespace BusinessApp.WebApi.Json
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.ComponentModel;

    /// <summary>
    /// Converts comma separate values into List
    /// </summary>
    /// <example>/resource?ids=1,2,3</example>
    public class CsvJsonConverter<T> : JsonConverter
    {
        private readonly TypeConverter converter;

        public override bool CanConvert(Type objectType) =>
            typeof(IEnumerable<T>).IsAssignableFrom(objectType);

        public CsvJsonConverter(TypeConverter converter = null)
        {
            this.converter = converter ?? TypeDescriptor.GetConverter(typeof(T));

            if (!this.converter.CanConvertFrom(typeof(string)))
            {
                throw new NotSupportedException("Type converter must be able to coonvert from a string");
            }
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.Value == null) return new List<T>();

            if (reader.Value is string str)
            {
                return new List<T>(str.Split(',').Select(s => (T)converter.ConvertFromString(s)));
            }

            var jObject = JObject.Load(reader);

            return jObject.ToObject(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("Can only read CSVs");
        }
    }
}
