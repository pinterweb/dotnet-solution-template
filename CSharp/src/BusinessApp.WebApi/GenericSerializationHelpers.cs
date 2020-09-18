namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Web;
    using BusinessApp.App;
    using BusinessApp.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public static class GenericSerializationHelpers<T>
    {
        private static readonly IDictionary<string, Type> propertyCache;

        static GenericSerializationHelpers()
        {
            propertyCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            RecurseAllProperties(typeof(T), "");
        }

        public static Stream DeserializeUri(HttpContext context, ISerializer serializer)
        {
            var collection = HttpUtility.ParseQueryString(context.Request.QueryString.Value);

            foreach (var r in context.GetRouteData().Values)
            {
                collection.Add(r.Key, r.Value.ToString());
            }

            var stream = new MemoryStream();

            if (collection.HasKeys())
            {
                serializer.Serialize(
                    stream,
                    CreateDictionary("", collection.AllKeys
                    .Where(key => !string.IsNullOrWhiteSpace(key) && propertyCache.ContainsKey(key))
                    .ToDictionary(key => key, key => collection[key])));

                stream.Position = 0;
            }

            return stream;
        }

        private static void RecurseAllProperties(Type type, string prefix)
        {
            foreach (var property in type.GetProperties())
            {
                var appClass = property.PropertyType.IsClass
                    && property.PropertyType.Namespace.StartsWith("BusinessApp");

                // prevent infinite recursion
                if (appClass && typeof(T) != property.PropertyType)
                {
                    RecurseAllProperties(property.PropertyType, prefix + property.Name + ".");
                }
                else
                {
                    propertyCache.Add(prefix + property.Name, property.PropertyType);
                }
            }
        }

        private static IDictionary<string, object> CreateDictionary(string prefix,
            IDictionary<string, string> dictionary)
        {
            var data =
                from kvp in dictionary
                 let properties = kvp.Key.Split('.')
                 group kvp by properties[0] into parent
                 select !parent.First().Key.Contains(".")
                     ? new KeyValuePair<string, object>(parent.Key,
                         ConvertStringToType(prefix + parent.Key, parent.First().Value))
                     : new KeyValuePair<string, object>(parent.Key,
                         CreateDictionary(prefix + parent.Key + ".", FilterByPropertyName(dictionary, parent.Key)));

            return data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static object ConvertStringToType(string key, string val)
        {
            var cachedType = propertyCache[key];
            bool isIEnumerable = cachedType.IsGenericIEnumerable();

            if (isIEnumerable)
            {
                var manyItems = val.Contains(',') ? val.Split(',') : new[] { val };
                var singItemConverter = TypeDescriptor.GetConverter(
                    cachedType.GetGenericArguments()[0]);
                return manyItems.Select(i => singItemConverter.ConvertFromInvariantString(i));
            }

            var converter = TypeDescriptor.GetConverter(cachedType);
            return converter.ConvertFromInvariantString(val);
        }

        private static IDictionary<string, string> FilterByPropertyName(
            IDictionary<string, string> dictionary, string propertyName)
        {
            string prefix = propertyName + ".";
            return dictionary.Keys
                .Where(key => key.StartsWith(prefix))
                .ToDictionary(key => key.Substring(prefix.Length), key => dictionary[key]);
        }
    }
}
