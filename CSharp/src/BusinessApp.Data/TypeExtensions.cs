using System;
using System.Collections.Generic;

namespace BusinessApp.Data
{
    public static class TypeExtensions
    {
        public static bool IsGenericIEnumerable(this Type type)
        {
            if (!type.IsConstructedGenericType) return false;

            return type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
