using System;
using System.Collections.Generic;

namespace BusinessApp.Infrastructure
{
    public static class TypeExtensions
    {
        public static bool IsGenericIEnumerable(this Type type)
            => type.IsConstructedGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }
}
