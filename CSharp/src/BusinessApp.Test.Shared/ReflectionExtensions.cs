using System;
using System.Reflection;

namespace BusinessApp.Test.Shared
{
    public static class ReflectionExtensions
    {
        public static T GetProp<T>(this T obj, string propName)
        {
            return (T)obj.GetType()
                .GetProperty(propName)
                .GetValue(obj);
        }

        public static T SetProp<T>(this T obj, string propertyName, object value)
            where T : class
        {
            PropertyInfo getProp(Type targetType)
            {
                return targetType
                    .GetProperty(propertyName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            };

            var currentObjType = obj.GetType();
            var prop = getProp(currentObjType);

            while (
                (!prop.CanWrite && currentObjType.BaseType != null) ||
                (prop == null && currentObjType.BaseType != null)
            )
            {
                currentObjType = currentObjType.BaseType;
                prop = getProp(currentObjType);
            }

            prop.SetValue(obj, value);

            return obj;
        }
    }
}
