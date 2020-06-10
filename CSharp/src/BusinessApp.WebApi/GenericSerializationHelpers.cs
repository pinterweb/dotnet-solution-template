namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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
            var props = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.PropertyType);

            propertyCache = new Dictionary<string, Type>(props, StringComparer.OrdinalIgnoreCase);
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
                    collection.AllKeys
                    .Where(key => !string.IsNullOrWhiteSpace(key) && propertyCache.ContainsKey(key))
                    .ToDictionary(key => key, key =>
                    {
                        var val = collection[key];

                        // invalid query string e.g. ?item=foo&&area=bar
                        if (key == null) return null;

                        var cacheValue = propertyCache[key];
                        bool isIEnumerable = cacheValue.IsGenericIEnumerable();

                        if (isIEnumerable)
                        {
                            var manyItems = val.Contains(',') ? val.Split(',') : new[] { val };
                            var singItemConverter = TypeDescriptor.GetConverter(
                                cacheValue.GetGenericArguments()[0]);
                            return manyItems.Select(i => singItemConverter.ConvertFromInvariantString(i));
                        }

                        // TODO if they send an ienumerable, but the property is not this will error
                        var converter = TypeDescriptor.GetConverter(cacheValue);
                        return converter.ConvertFromInvariantString(val);
                    })
                );

                stream.Position = 0;
            }

            return stream;
        }
    }
}
