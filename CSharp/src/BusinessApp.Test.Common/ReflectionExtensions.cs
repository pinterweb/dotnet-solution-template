namespace BusinessApp.Test.Common
{
    using System;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        public static T GetProp<T>(this T obj, string propName)
        {
            return (T)obj.GetType()
                .GetProperty(propName)
                .GetValue(obj);
        }
    }
}

