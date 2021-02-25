namespace BusinessApp.WebApi.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Converts to and from <see cref="IDictionary{string, string}" />
    /// </summary>
    public class NewtonsoftIDictionaryJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            typeof(IDictionary<string, string>).IsAssignableFrom(objectType);

        public override bool CanWrite => true;
        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dict = new Dictionary<string, object>();

            if (reader.TokenType == JsonToken.Null) return dict;

            var jObject = JObject.Load(reader);
            var converted =  (IDictionary<string, object>)jObject.ToObject(objectType);

            foreach(var kvp in converted)
            {
              var key = Char.ToUpperInvariant(kvp.Key[0]) + kvp.Key.Substring(1);
              dict[key] = kvp.Value;
            }

            return dict;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = value as IDictionary<string, string>;

            if (!dict.Any(d => d.Key.Contains(".")))
            {
                JToken.FromObject(value).WriteTo(writer);
            }
            else
            {
                JObject.Parse(ConvertDictionaryToJson(dict)).WriteTo(writer);
            }
        }

        public static string ConvertQueryStringToJson(string query)
        {
            var collection = HttpUtility.ParseQueryString(query);
            var dictionary = collection.AllKeys.ToDictionary(key => key, key => collection[key]);
            return ConvertDictionaryToJson(dictionary);
        }

        private static string ConvertDictionaryToJson(IDictionary<string, string> dictionary)
        {
            var propertyNames =
                from key in dictionary.Keys
                let index = key.IndexOf(value: '.')
                select index < 0 ? key : key.Substring(0, index);

            var data =
                from propertyName in propertyNames.Distinct()
                let json = dictionary.ContainsKey(propertyName)
                    ? HttpUtility.JavaScriptStringEncode(dictionary[propertyName], addDoubleQuotes: true)
                    : ConvertDictionaryToJson(FilterByPropertyName(dictionary, propertyName))
                select HttpUtility.JavaScriptStringEncode(propertyName, addDoubleQuotes: true) + ": " + json;

            return "{ " + string.Join(", ", data) + " }";
        }

        private static Dictionary<string, string> FilterByPropertyName(IDictionary<string, string> dictionary,
            string propertyName)
        {
            string prefix = propertyName + ".";
            return dictionary.Keys
                .Where(key => key.StartsWith(prefix))
                .ToDictionary(key => key.Substring(prefix.Length), key => dictionary[key]);
        }
    }
}
