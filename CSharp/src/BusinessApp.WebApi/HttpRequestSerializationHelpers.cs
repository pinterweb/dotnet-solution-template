using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BusinessApp.WebApi
{
#pragma warning disable CA1000
    /// <summary>
    /// Helper methods to serialize an http request to a <typeparam name="T" />
    /// </summary>
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

        public static T? SerializeRouteAndQueryValues(HttpRequest request, ISerializer serializer)
        {
            var queryArgs = new Dictionary<string, object?>(request.RouteValues);
            var collection = request.QueryString.Value != null
                ? HttpUtility.ParseQueryString(request.QueryString.Value)
                : new NameValueCollection();

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

        public static void SetProperties(T target, RouteValueDictionary properties)
        {
            foreach (var kvp in properties)
            {
                if (setterCache.TryGetValue(kvp.Key, out var setterGetter))
                {
                    var converter = TypeDescriptor.GetConverter(setterGetter.Item1);

                    if (kvp.Value != null && converter.CanConvertFrom(kvp.Value.GetType()))
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
                    && property.PropertyType.Namespace != null
                    && property.PropertyType.Namespace.StartsWith("BusinessApp", StringComparison.OrdinalIgnoreCase);

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
                var setter = BuildSetter(property);

                if (setter != null)
                {
                    setterCache.Add(property.Name, (property.PropertyType, setter));
                }
            }
        }

        private static IDictionary<string, object> CreateDictionary(string prefix,
            IDictionary<string, object?> dictionary)
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
            var isIEnumerable = cachedType.IsGenericIEnumerable();

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

        private static IDictionary<string, object?> FilterByPropertyName(
            IDictionary<string, object?> dictionary, string propertyName)
        {
            var prefix = propertyName + ".";

            return dictionary.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.InvariantCulture))
                .ToDictionary(key => key[prefix.Length..], key => dictionary[key]);
        }

        public static Action<T, object>? BuildSetter(PropertyInfo p)
        {
            var setter = p.GetSetMethod(true);

            if (setter == null || p.DeclaringType == null) return null;

            var instance = Expression.Parameter(p.DeclaringType, "instance");
            var valParam = Expression.Parameter(typeof(object), "p");
            var caller = Expression.Call(instance,
                    setter,
                    Expression.Convert(valParam, p.PropertyType));

            var lambda = Expression.Lambda<Action<T, object>>(caller, instance, valParam);

            return lambda.Compile();
        }

        private static bool IsIEntityId(Type type) => typeof(IEntityId).IsAssignableFrom(type);
    }
#pragma warning restore CA1000
}
