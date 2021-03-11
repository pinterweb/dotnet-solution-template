namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web;
    using BusinessApp.App;
    using BusinessApp.Data;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestSerializationHelpers<T>
    {
        private static readonly IDictionary<string, Type> propertyCache;
        private static readonly IDictionary<string, (Type, Action<T, object>)> setterCache;

        static HttpRequestSerializationHelpers()
        {
            propertyCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            setterCache = new Dictionary<string, (Type, Action<T, object>)>(StringComparer.OrdinalIgnoreCase);

            RecurseAllProperties(typeof(T), "");
            GetTopLevelProperties(typeof(T));
        }

        public static T SerializeRouteAndQueryValues(HttpRequest request, ISerializer serializer)
        {
            var queryArgs = new Dictionary<string, object>(request.RouteValues);
            var collection = HttpUtility.ParseQueryString(request.QueryString.Value);

            foreach (string r in collection)
            {
                if (!string.IsNullOrWhiteSpace(r) && !queryArgs.ContainsKey(r))
                {
                    queryArgs.Add(r, collection[r]);
                }
            }

            var dictionaryOfValues = CreateDictionary("", queryArgs
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key)
                    && propertyCache.ContainsKey(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            var data = serializer.Serialize(dictionaryOfValues);

            return serializer.Deserialize<T>(data);
        }

        public static void SetProperties(T target, IDictionary<string, object> properties)
        {
            foreach (var kvp in properties)
            {
                if (setterCache.TryGetValue(kvp.Key, out (Type, Action<T, object>) setterGetter))
                {
                    var converter = TypeDescriptor.GetConverter(setterGetter.Item1);

                    if (converter.CanConvertFrom(kvp.Value.GetType()))
                    {
                        setterGetter.Item2(
                            target,
                            converter.ConvertFrom(kvp.Value)
                        );
                    }
                }
            }
        }

        private static void RecurseAllProperties(Type type, string prefix)
        {
            foreach (var property in type.GetProperties())
            {
                var appClass = !IsIEntityId(property.PropertyType)
                    && property.PropertyType.IsClass
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

        private static void GetTopLevelProperties(Type type)
        {
            foreach (var property in type.GetProperties())
            {
                setterCache.Add(property.Name, (property.PropertyType, BuildSetter(property)));
            }
        }

        private static IDictionary<string, object> CreateDictionary(string prefix,
            IDictionary<string, object> dictionary)
        {
            var data =
                from kvp in dictionary
                 let properties = kvp.Key.Split('.')
                 group kvp by properties[0] into parent
                 select !parent.First().Key.Contains(".")
                     ? new KeyValuePair<string, object>(parent.Key,
                         ConvertToType(prefix + parent.Key, parent.First().Value))
                     : new KeyValuePair<string, object>(parent.Key,
                         CreateDictionary(prefix + parent.Key + ".", FilterByPropertyName(dictionary, parent.Key)));

            return data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static object ConvertToType(string key, object val)
        {
            var cachedType = propertyCache[key];
            bool isIEnumerable = cachedType.IsGenericIEnumerable();

            if (isIEnumerable && val is string v)
            {
                var manyItems = v.Contains(',') ? v.Split(',') : new[] { v };
                var singItemConverter = TypeDescriptor.GetConverter(
                    cachedType.GetGenericArguments()[0]);
                return manyItems.Select(i => singItemConverter.ConvertFromInvariantString(i));
            }

            var converter = TypeDescriptor.GetConverter(cachedType);
            return converter.ConvertFrom(val);
        }

        private static IDictionary<string, object> FilterByPropertyName(
            IDictionary<string, object> dictionary, string propertyName)
        {
            string prefix = propertyName + ".";
            return dictionary.Keys
                .Where(key => key.StartsWith(prefix))
                .ToDictionary(key => key.Substring(prefix.Length), key => dictionary[key]);
        }

        public static Action<T, object> BuildSetter(PropertyInfo p)
        {
            var instance = Expression.Parameter(p.DeclaringType, "instance");

            var setter = p.GetSetMethod(true);
            var valParam = Expression.Parameter(typeof(object), "p");
            var caller = Expression.Call(instance,
                    setter,
                    Expression.Convert(valParam, p.PropertyType));

            var lambda = Expression.Lambda<Action<T, object>>(caller, instance, valParam);

            return lambda.Compile();
        }

        private static bool IsIEntityId(Type type)
        {
            return typeof(IEntityId).IsAssignableFrom(type);
        }
    }
}